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

function emulator.write_byte(address, value, domain)
    memory.writebyte(translate_address(address, domain), value, domain)
end

function emulator.write_uint16(address, value, domain)
    memory.writeword(translate_address(address, domain), value, domain)
end

function emulator.get_rom_name()
    return ""
end

function emulator.get_rom_hash()
    return ""
end

return emulator;
