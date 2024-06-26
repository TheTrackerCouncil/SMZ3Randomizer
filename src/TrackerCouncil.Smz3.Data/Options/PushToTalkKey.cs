using System.ComponentModel;

namespace TrackerCouncil.Smz3.Data.Options;

public enum PushToTalkKey : ushort
{
    [Description("Escape")]
    KeyEscape = 0x001B,

    [Description("F1")]
    KeyF1 = 0x0070,

    [Description("F2")]
    KeyF2 = 0x0071,

    [Description("F3")]
    KeyF3 = 0x0072,

    [Description("F4")]
    KeyF4 = 0x0073,

    [Description("F5")]
    KeyF5 = 0x0074,

    [Description("F6")]
    KeyF6 = 0x0075,

    [Description("F7")]
    KeyF7 = 0x0076,

    [Description("F8")]
    KeyF8 = 0x0077,

    [Description("F9")]
    KeyF9 = 0x0078,

    [Description("F10")]
    KeyF10 = 0x0079,

    [Description("F11")]
    KeyF11 = 0x007A,

    [Description("F12")]
    KeyF12 = 0x007B,

    [Description("F13")]
    KeyF13 = 0xF000,

    [Description("F14")]
    KeyF14 = 0xF001,

    [Description("F15")]
    KeyF15 = 0xF002,

    [Description("F16")]
    KeyF16 = 0xF003,

    [Description("F17")]
    KeyF17 = 0xF004,

    [Description("F18")]
    KeyF18 = 0xF005,

    [Description("F19")]
    KeyF19 = 0xF006,

    [Description("F20")]
    KeyF20 = 0xF007,

    [Description("F21")]
    KeyF21 = 0xF008,

    [Description("F22")]
    KeyF22 = 0xF009,

    [Description("F23")]
    KeyF23 = 0xF00A,

    [Description("F24")]
    KeyF24 = 0xF00B,

    [Description("`")]
    KeyBackQuote = 0x00C0,

    [Description("0")]
    Key0 = 0x0030,

    [Description("1")]
    Key1 = 0x0031,

    [Description("2")]
    Key2 = 0x0032,

    [Description("3")]
    Key3 = 0x0033,

    [Description("4")]
    Key4 = 0x0034,

    [Description("5")]
    Key5 = 0x0035,

    [Description("6")]
    Key6 = 0x0036,

    [Description("7")]
    Key7 = 0x0037,

    [Description("8")]
    Key8 = 0x0038,

    [Description("9")]
    Key9 = 0x0039,

    [Description("-")]
    KeyMinus = 0x002D,

    [Description("=")]
    KeyEquals = 0x003D,

    [Description("Backspace")]
    KeyBackspace = 0x0008,

    [Description("Tab")]
    KeyTab = 0x0009,

    [Description("Caps Lock")]
    KeyCapsLock = 0x0014,

    [Description("A")]
    KeyA = 0x0041,

    [Description("B")]
    KeyB = 0x0042,

    [Description("C")]
    KeyC = 0x0043,

    [Description("D")]
    KeyD = 0x0044,

    [Description("E")]
    KeyE = 0x0045,

    [Description("F")]
    KeyF = 0x0046,

    [Description("G")]
    KeyG = 0x0047,

    [Description("H")]
    KeyH = 0x0048,

    [Description("I")]
    KeyI = 0x0049,

    [Description("J")]
    KeyJ = 0x004A,

    [Description("K")]
    KeyK = 0x004B,

    [Description("L")]
    KeyL = 0x004C,

    [Description("M")]
    KeyM = 0x004D,

    [Description("N")]
    KeyN = 0x004E,

    [Description("O")]
    KeyO = 0x004F,

    [Description("P")]
    KeyP = 0x0050,

    [Description("Q")]
    KeyQ = 0x0051,

    [Description("R")]
    KeyR = 0x0052,

    [Description("S")]
    KeyS = 0x0053,

    [Description("T")]
    KeyT = 0x0054,

    [Description("U")]
    KeyU = 0x0055,

    [Description("V")]
    KeyV = 0x0056,

    [Description("W")]
    KeyW = 0x0057,

    [Description("X")]
    KeyX = 0x0058,

    [Description("Y")]
    KeyY = 0x0059,

    [Description("Z")]
    KeyZ = 0x005A,

    [Description("[")]
    KeyOpenBracket = 0x005B,

    [Description("]")]
    KeyCloseBracket = 0x005C,

    [Description("\\")]
    KeyBackslash = 0x005D,

    [Description(";")]
    KeySemicolon = 0x003B,

    [Description("'")]
    KeyQuote = 0x00DE,

    [Description("Enter")]
    KeyEnter = 0x000A,

    [Description(",")]
    KeyComma = 0x002C,

    [Description(".")]
    KeyPeriod = 0x002E,

    [Description("/")]
    KeySlash = 0x002F,

    [Description("Space")]
    KeySpace = 0x0020,

    [Description("Print Screen")]
    KeyPrintScreen = 0x009A,

    [Description("Scroll Lock")]
    KeyScrollLock = 0x0091,

    [Description("Pause")]
    KeyPause = 0x0013,

    [Description("Cancel")]
    KeyCancel = 0x00D3,

    [Description("Help")]
    KeyHelp = 0x009F,

    [Description("Insert")]
    KeyInsert = 0x009B,

    [Description("Delete")]
    KeyDelete = 0x007F,

    [Description("Home")]
    KeyHome = 0x0024,

    [Description("End")]
    KeyEnd = 0x0023,

    [Description("Page Up")]
    KeyPageUp = 0x0021,

    [Description("Page Down")]
    KeyPageDown = 0x0022,

    [Description("Up Arrow")]
    KeyUp = 0x0026,

    [Description("Left Arrow")]
    KeyLeft = 0x0025,

    [Description("Right Arrow")]
    KeyRight = 0x0027,

    [Description("Down Arrow")]
    KeyDown = 0x0028,

    [Description("Num Lock")]
    KeyNumLock = 0x0090,

    [Description("Num-Pad Clear")]
    KeyNumPadClear = 0x000C,

    [Description("Num-Pad /")]
    KeyNumPadDivide = 0x006F,

    [Description("Num-Pad *")]
    KeyNumPadMultiply = 0x006A,

    [Description("Num-Pad -")]
    KeyNumPadSubtract = 0x006D,

    [Description("Num-Pad =")]
    KeyNumPadEquals = 0x007C,

    [Description("Num-Pad +")]
    KeyNumPadAdd = 0x006B,

    [Description("Num-Pad Enter")]
    KeyNumPadEnter = 0x007D,

    [Description("Num-Pad Decimal")]
    KeyNumPadDecimal = 0x006E,

    [Description("Num-Pad Separator")]
    KeyNumPadSeparator = 0x006C,

    [Description("Num-Pad 0")]
    KeyNumPad0 = 0x0060,

    [Description("Num-Pad 1")]
    KeyNumPad1 = 0x0061,

    [Description("Num-Pad 2")]
    KeyNumPad2 = 0x0062,

    [Description("Num-Pad 3")]
    KeyNumPad3 = 0x0063,

    [Description("Num-Pad 4")]
    KeyNumPad4 = 0x0064,

    [Description("Num-Pad 5")]
    KeyNumPad5 = 0x0065,

    [Description("Num-Pad 6")]
    KeyNumPad6 = 0x0066,

    [Description("Num-Pad 7")]
    KeyNumPad7 = 0x0067,

    [Description("Num-Pad 8")]
    KeyNumPad8 = 0x0068,

    [Description("Num-Pad 9")]
    KeyNumPad9 = 0x0069,

    [Description("Left Shift")]
    KeyLeftShift = 0xA010,

    [Description("Right Shift")]
    KeyRightShift = 0xB010,

    [Description("Left Control")]
    KeyLeftControl = 0xA011,

    [Description("Right Control")]
    KeyRightControl = 0xB011,

    [Description("Left Alt")]
    KeyLeftAlt = 0xA012,

    [Description("Right Alt")]
    KeyRightAlt = 0xB012,

    [Description("Context Menu")]
    KeyContextMenu = 0x020D,
}
