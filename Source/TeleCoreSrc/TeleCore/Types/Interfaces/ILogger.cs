using UnityEngine;

namespace TeleCore.Types.Interfaces;

public interface ILogger
{
    void Message(string msg);
    void Message(string msg, Color color);
    void Warning(string msg);
    void Error(string msg);
    void ErrorOnce(string msg, int id);
    void Debug(string msg, bool flag = true);
    void DebugSuccess(string msg);
    void DebugOnce(string msg, int hash);
}