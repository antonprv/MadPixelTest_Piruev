// Created by Anton Piruev in 2026. 
// Any direct commercial use of derivative work is strictly prohibited.

public sealed class CallSite
{
  public string ClassName { get; }
  public string FunctionName { get; }
  public string Path { get; }
  public int Line { get; }

  public CallSite(string className, string functionName, string path, int line)
  {
    ClassName = className;
    FunctionName = functionName;
    Path = path;
    Line = line;
  }
}