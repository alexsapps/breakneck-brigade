using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pencil.Gaming;
using Pencil.Gaming.Graphics;
using SousChef;

namespace Breakneck_Brigade
{
    class Program
    {
        /**
         * Runs GLFW initialization code, terminiating if initialization failed
         */
        static void InitGLFW()
        {
            if(!Glfw.Init())
            {
                Console.Error.WriteLine("ERROR: GLFW Initialization failed!");
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
           // LoadConfig loader = new LoadConfig("../../Config/ConfigFiles.xml");

            InitGLFW();
            GlfwWindowPtr mainWindow = Glfw.CreateWindow(640, 480, "Breakneck Brigade", GlfwMonitorPtr.Null, GlfwWindowPtr.Null);
            Glfw.MakeContextCurrent(mainWindow);

            while (!Glfw.WindowShouldClose(mainWindow))
            {
                int width, height;
                Glfw.GetFramebufferSize(mainWindow, out width, out height);
                float ratio = (float)width / height;

                GL.Enable(EnableCap.Lighting);

                Matrix4 matrix4 = new Matrix4();
                //GL.LoadMatrix(matrix4.m);
             
                GL.Viewport(0, 0, width, height);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                GL.MatrixMode(MatrixMode.Projection);
                GL.LoadIdentity();
                GL.Ortho(-ratio, ratio, -1.0, 1.0, 1.0, -1.0);

                GL.MatrixMode(MatrixMode.Modelview);
                GL.LoadIdentity();
                GL.Rotate(Glfw.GetTime() * 50.0, 0, 0, 1.0);

                GL.Begin(BeginMode.Triangles);
                GL.Color3(.1, 0.0, 0.0);
                GL.Vertex3(-.6, -.4, 0.0);
                GL.Color3(0.0, .1, 0.0);
                GL.Vertex3(.6, -.4, 0.0);
                GL.Color3(0.0, 0.0, .10);
                GL.Vertex3(0.0, .6, 0.0);
                GL.End();

                Glfw.SwapBuffers(mainWindow);
                Glfw.PollEvents();
            }

            Glfw.DestroyWindow(mainWindow);
            Glfw.Terminate();
            Environment.Exit(0);
        }
    }
}
