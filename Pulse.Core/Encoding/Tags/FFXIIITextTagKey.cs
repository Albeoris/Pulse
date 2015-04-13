namespace Pulse.Core
{
    //0xF1
    public enum FFXIIITextTagKey : byte
    {
        Cross = 0x40, // A, Enter
        Circle = 0x41, // B, Backspace
        Square = 0x42, // X, X
        Triangle = 0x43, // Y, E
        
        Start = 0x44, // Start, 0
        Select = 0x45, // Select, 1
        
        L1 = 0x46, // LB, Tab
        R1 = 0x47, // RB, C
        L2 = 0x48, // LT 
        R2 = 0x49, // RT 

        Left = 0x4A,
        Down = 0x4B,
        Right = 0x4C,
        Up = 0x4D,
        
        LSLeft = 0x4E,
        LSDown = 0x4F,
        LSRight = 0x50,
        LSUp = 0x51,
        LSLeftRight = 0x52,
        LSUpDow = 0x53,
        LSPress = 0x54,

        RSPress = 0x55, // RSPress, 5
        RSLeft = 0x56,
        RSDown = 0x57, // RSDown, 2
        RSRight = 0x58,
        RSUp = 0x59, // RSUp, 8
        
        DPad = 0x5A, // DPad, Arrows
        Analog = 0x5B, // Analog, Zoom in map in texttagkey.
        LStick = 0x5C, // Move character
        NPad = 0x5D, // Rotate camera
        
        //todo UpDownPad
        LeftRightAnalogic = 0x5E, // LeftRight Analogic
        LeftRightPad = 0x5F, //LeftRight Pad

        Arrows = 0x60 
    }
}