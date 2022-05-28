local socket = require('socket.core')
local json = require('json')

local HOST_ADDRESS = '127.0.0.1'
local HOST_PORT = 6969
local RECONNECT_DELAY = 5

local tcp = nil
local connected = false
local lastConnectionAttempt = os.time()
local part = nil

local function ends_with(str, ending)
   return ending == "" or str:sub(-#ending) == ending
end

local function message(msg)
	emu.message(msg)
	print(msg)
end

local function check_for_message()
	local data, err, tempPart = tcp:receive(n, part)
	if data == nil then
		if err ~= 'timeout' then
			message('Connection lost:' .. err)
			connected = false
			connect()
		else
			--print('null', err)
			--print('null', part)
			part = tempPart
		end
	else
		--print('part')
		part = nil
	end
	-- print('part', part)
	-- print('data', data)
	if (ends_with(part, "\0")) then
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
	local address = data['Address']
	local length = data['Length']
	local bytes = nil
	
	-- print('action', action);
	-- print('address', address);
	-- print('length', length);
	if (action == 'read_block') then
		bytes = memory.readbyterange(address, length)
	end
	
	local result = {
		Action = action,
		Address = address,
		Length = length,
		Bytes = bytes
	}
	-- print('result', json.encode(result))
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
		message('Connection established')
		tcp:settimeout(0)
		connected = true
	else
		message('Failed to open socket:' .. err)
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
			-- local dataObject = json.decode(data);
			-- print(data)
			-- local ret, err = tcp:send("hello\n")
			-- if ret == nil then
			-- 	print('Failed to send:', err)
			-- end
			message = check_for_message()
		end
	else
		local currentTime = os.time()
		if lastConnectionAttempt + RECONNECT_DELAY <= currentTime then
			connect()
		end
	end
end

emu.registerbefore(on_tick)
connect()

