using System;
public static class StaticClass {}
public static class Extensions {
    public static void Method(this StaticClass s) {}
}
class Program {
    static void Main() {
        StaticClass s = null; // Can't even do this, but the extension method signature itself might be valid?
    }
}
