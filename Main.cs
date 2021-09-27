using System;
using Gtk;
using Project;

namespace Project {
  public class MainClass {
    public static void Main(){
      Application.Init();
      new Program();
      Application.Run();
    }
  }
}
