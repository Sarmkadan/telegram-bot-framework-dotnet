using System;
namespace Test {
    public class Model {}
    public static class Ext1 {
        public static void Method(this Model m) {}
    }
    public static class Ext2 {
        public static void Method(this object o) {}
    }
    class Program {
        static void Main() {
            Model m = new Model();
            m.Method(); // Call extension method
        }
    }
}
