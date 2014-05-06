using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Glfw;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade
{
    /// <summary>
    /// Input interface that stores the keypress states of keyboard keys and mouse.
    /// </summary>
    class InputManager
    {
        GlobalsConfigFile globalConfig;

        // Game-related stuff
        /// <summary>
        /// States of keys being tracked
        /// </summary>
        public HashSet<GlfwKeys> keys = new HashSet<GlfwKeys>();

        /// <summary>
        /// Current rotation amount for current update cycle
        /// </summary>
        Vector4 rot = new Vector4();

        /// <summary>
        /// fpsMode means locking mouse to center of screen and hiding the cursor
        /// </summary>
        public bool fpsMode = false;        

        /// <summary>
        /// Variables for storing the "center" mouse position
        /// </summary>
        public int originX = 0;
        public int originY = 0;

        /// <summary>
        /// Mouse sensitivity modifier
        /// </summary>
        float sens = 1.0f;

        /// <summary>
        /// Should the mouse Y axis be inverted? Most first person games support this.
        /// </summary>
        bool invertY = false;

        /// <summary>
        /// Variables for GLFW callbacks
        /// </summary>
        private Glfw.GLFWkeyfun         keyboardCallback;
        private Glfw.GLFWmouseposfun    mouseMoveCallback;
        private Glfw.GLFWmousebuttonfun mouseButtonCallback;

        public InputManager()
        {
            // callback for keyboard; will trigger KeyboardInput() on every keypress or release
            keyboardCallback = KeyboardInput;               //Need to follow this convention for declaring callbacks so C# does not GC the reference immediately
            Glfw.glfwSetKeyCallback(keyboardCallback);
            // callback for mouse; will trigger MousePos() on every mouse movement
            mouseMoveCallback = MousePos;
            Glfw.glfwSetMousePosCallback(mouseMoveCallback);
            // callback for mouse; will trigger MouseButton() on every mouse click/release
            mouseButtonCallback = MouseButton;
            Glfw.glfwSetMouseButtonCallback(mouseButtonCallback);

            globalConfig = new GlobalsConfigFolder(BBXml.GetLocalConfigFolder()).Open("keys.xml");

            //keys[GLFW_KEY_W]                = false;
            //keys[GLFW_KEY_A]                = false;
            //keys[GLFW_KEY_S]                = false;
            //keys[GLFW_KEY_D]                = false;
            //keys[GLFW_KEY_SPACE]            = false;
            //keys[GLFW_KEY_ESCAPE]           = false;
            //keys[GLFW_KEY_LEFT]             = false;
            //keys[GLFW_KEY_RIGHT]            = false;
            //keys[GLFW_KEY_UP]               = false;
            //keys[GLFW_KEY_DOWN]             = false;

            //keys[GLFW_MOUSE_BUTTON_LEFT]    = false;
            //keys[GLFW_MOUSE_BUTTON_RIGHT]   = false;

            Console.WriteLine("This happened.");
        }

        /// <summary>
        /// Callback method for keyboard inputs
        /// </summary>
        /// <param name="key">Keyboard code</param>
        /// <param name="action">GLFW_PRESS or GLFW_RELEASE</param>
        void KeyboardInput(int temp, int action)
        {
            bool pressed = (action == Glfw.GLFW_PRESS);
            GlfwKeys key = remap(temp);
            if (pressed)
                keys.Add(key); // = pressed;
            else
                keys.Remove(key);

            //Console.WriteLine("key " + key.ToString() + (pressed ? " pressed." : " released."));
        }

        /// <summary>
        /// Callback method that updates the mouse delta
        /// </summary>
        /// <param name="x">position of mouse x position at callback</param>
        /// <param name="y">position of mouse y position at callback</param>
        void MousePos(int x, int y)
        {
            if (fpsMode)
            {
                Glfw.glfwDisable(Glfw.GLFW_MOUSE_CURSOR);
                // Get difference of mouse pos & origin
                int tempX = originX - x;
                int tempY = originY - y;

                // add movement to rotation modified by sensitivity setting
                // positive X movement negatively rotates around Y axis
                rot[1] -= tempX * sens;

                if (!invertY)
                {
                    // positive Y movement negatively rotates around X axis with no invert
                    rot[0] -= tempY * sens;
                }
                else
                {
                    rot[0] += tempY * sens;
                }

                // Reset mouse to origin
                Glfw.glfwSetMousePos(originX, originY);
            }
            else
            {
                Glfw.glfwEnable(Glfw.GLFW_MOUSE_CURSOR);
            }
        }

        /// <summary>
        /// Sets the initial position of the mouse cursor for FPS mode, and saves the values for later use.
        /// </summary>
        void MousePosInit()
        {
            int width, height;
            Glfw.glfwGetWindowSize(out width, out height);

            Glfw.glfwSetMousePos(width/2, height/2);
            Glfw.glfwGetMousePos(out originX, out originY);

            // To-be-implemented; get sens from config
            sens = float.Parse(globalConfig.GetSetting("sens", 0.1f));
            // To-be-implemented; get invertY from config
            invertY = bool.Parse(globalConfig.GetSetting("invertY", false));
            // To-be-implemented; swap left/right click
            /* if(swapFlag)
             * {
             *      mouse button left = mouse button 2;
             *      mouse button right = mouse button 1;
             * }
             */
        }

        /// <summary>
        /// Callback for mouse button presses
        /// </summary>
        /// <param name="button">Mouse button code</param>
        /// <param name="action">GLFW_PRESS or GLFW_RELEASE</param>
        void MouseButton(int temp, int action)
        {
            var button = (GlfwKeys)temp;
            bool pressed = (action == Glfw.GLFW_PRESS);
            if (pressed)
                keys.Add(button);
            else
                keys.Remove(button);
        }

        public void Clear()
        {
            rot[0] = 0.0f;
            rot[1] = 0.0f;
        }

        /// <summary>
        /// Remaps rebound keys from config file to locally defined values.
        /// </summary>
        /// <param name="key">value of key passed to callback method</param>
        /// <returns>value of key as defined by config file or default value if not defined</returns>
        private GlfwKeys remap(int key)
        {
            // if config load is broken,
            // return key;
            return (GlfwKeys)int.Parse(globalConfig.GetSetting(key.ToString(), key));
        }

        /// <summary>
        /// Methods for toggling FPS mouse mode.
        /// </summary>
        public void EnableFPSMode()
        {
            MousePosInit();
            Glfw.glfwDisable(Glfw.GLFW_MOUSE_CURSOR);
            fpsMode = true;
            
        }
        public void DisableFPSMode()
        {
            Glfw.glfwEnable(Glfw.GLFW_MOUSE_CURSOR);
            fpsMode = false;
        }

        /// <summary>
        /// Public access methods for getting current rotation amount
        /// </summary>
        public float GetRotX()
        {
            return rot[0];
        }

        public float GetRotY()
        {
            return rot[1];
        }

        /// <summary>
        /// More frequently called method for getting the rotation for the camera
        /// </summary>
        /// <param name="rotx"></param>
        /// <param name="roty"></param>
        public void GetRotAndClear(out float rotx, out float roty){
            rotx = rot[0];
            roty = rot[1];

            Clear();
        }

        public HashSet<GlfwKeys> GetKeys()
        {
            return keys;
        }

        public bool this[GlfwKeys key]
        {
            get
            {
                return keys.Contains(key);
            }
        }
    }

    class KeyHappenedEventArgs : EventArgs
    {
        public GlfwKeys Key { get; set; }
        public bool Up { get; set; }
        public bool Down { get; set; }
    }
}
