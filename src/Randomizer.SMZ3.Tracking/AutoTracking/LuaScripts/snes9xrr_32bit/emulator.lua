local base64 = require('base64')

local emulator = { }

local function base64_encode(bytes, length)
    local temp = ''
	for i = 1, length do
		temp = temp .. string.char(bytes[i])
	end
	return to_base64(temp)
end

local function translate_address(address, domain)
	if domain == "CARTRAM" then
		return address + 0x314000
	end
	return address
end

function emulator.start_tick(callback)
    emu.registerbefore(callback)
end

function emulator.print(message)
    emu.message(message)
	print(message)
end

function emulator.read_bytes(address, length, domain)
    return base64_encode(memory.readbyterange(translate_address(address, domain), length, domain), length)
end

return emulator;