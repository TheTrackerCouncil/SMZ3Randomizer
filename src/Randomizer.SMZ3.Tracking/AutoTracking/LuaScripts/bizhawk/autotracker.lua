local socket = require('socket.core')
local json = loadfile('json.lua')()
local emulator = loadfile('emulator.lua')()
local base64 = require('base64')

local HOST_ADDRESS = '127.0.0.1'
local HOST_PORT = 6969
local DISCONNECT_DELAY = 5
local RECONNECT_DELAY = 5

local tcp = nil
local connected = false
local lastConnectionAttempt = os.time()
local lastMessage = os.time()
local part = nil

local function ends_with(str, ending)
   return ending == "" or str:sub(-#ending) == ending
end

local function check_for_message()
	local data, err, tempPart = tcp:receive(n, part)
	if data == nil then
		if err ~= 'timeout' then
			emulator.print('Connection lost:' .. err)
			connected = false
		else
			part = tempPart
		end
	else
		part = nil
	end

	if part ~= nil and ends_with(part, "\0") then
		data = part
		part = nil
		return data
	else
		return nil
	end

end

local function process_message(message)
	local data = json.decode(message)
	local action = data['Action']
	local domain = data['Domain']
	local address = data['Address']
	local length = data['Length']
	
	local bytes = nil
	
	if (action == 'read_block') then
		bytes = emulator.read_bytes(address, length, domain)
	end

	local result = {
		Action = action,
		Address = address,
		Length = length,
		Bytes = bytes
	}

	-- print(json.encode(result))
	local ret, err = tcp:send(json.encode(result) .. "\n")
	if ret == nil then
	 	print('Failed to send:', err)
	end
end

local function connect()
	tcp = socket.tcp()
	lastConnectionAttempt = os.time()
	print('Attempting to connect')
	
	local ret, err = tcp:connect(HOST_ADDRESS, HOST_PORT)
	if ret == 1 then
		emulator.print('Connection established')
		tcp:settimeout(0)
		connected = true
        lastMessage = os.time()
	else
		emulator.print('Failed to open socket:' .. err)
		tcp:close()
		tcp = nil
		connected = false
	end
end

local function on_tick()
	if connected then
		local message = check_for_message()
		while message ~= nil do
			-- print(message)
			process_message(message);
            lastMessage = os.time()
			message = check_for_message()
		end

        local currentTime = os.time()
        if lastMessage + DISCONNECT_DELAY <= currentTime then
            connected = false
        end
	else
		local currentTime = os.time()
		if lastConnectionAttempt + RECONNECT_DELAY <= currentTime then
			connect()
		end
	end
end

emulator.start_tick(on_tick)
connect()

