local emulator = { }

local function base64_encode(bytes, length)
    local temp = ''
	for i = 0, length - 1 do
		temp = temp .. string.char(bytes[i])
	end
	return to_base64(temp)
end

local function translate_address(address, domain)
	if domain == "WRAM" then
		return address - 0x7e0000
	elseif domain == "CARTRAM" then
		return address - 0x700000
	elseif domain == "CARTROM" then
		return address
	end
	return address
end

function emulator.start_tick(callback)
    while true do
        callback()
        emu.yield()
    end
end

function emulator.print(message)
    gui.addmessage(message)
	print(message)
end

function emulator.read_bytes(address, length, domain)
    return base64_encode(memory.readbyterange(translate_address(address, domain), length, domain), length)
end

function emulator.write_uint8(address, value, domain)
  memory.write_u8(translate_address(address, domain), value, domain)
end

function emulator.write_uint16(address, value, domain)
	memory.write_u16_le(translate_address(address, domain), value, domain)
end

return emulator;
