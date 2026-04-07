using System;

namespace TeleCore.Network.IO;

[Flags]
public enum NetworkIOMode : byte
{
    None = 0,                   //00000000
    Input = 1,                  //00000001
    Output = 2,                 //00000010
    Visual = 8,                 //00001000
    Logical = 16 | TwoWay,      //00010011
    TwoWay = Input | Output,    //00000011

    ForRender = Visual | Input | Output
}