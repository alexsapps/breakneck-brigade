using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Glfw;
using Tao.OpenGl;
using SousChef;
using System.Threading;

namespace Breakneck_Brigade
{
    /// <summary>
    /// Input interface that stores the keypress states of keyboard keys and mouse.
    /// </summary>
    class InputManager
    {
        public BBLock Lock = new BBLock();

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
        private bool _fpsOk = false; //true if window has focus
        public bool FpsOk
        {
            get
            {
                return _fpsOk;
            }
            set
            {
                _fpsOk = value;
                updateGlfwMouseDisabled();
            }
        }

        private bool _captureMouse = true; // true if user wants their mouse captured
        public bool CaptureMouse
        {
            get
            {
                return _captureMouse;
            }
            set
            {
                _captureMouse = value;
                updateGlfwMouseDisabled();
            }
        }

        private bool _fpsMode = false; //prevent accidentally setting this variable by separating it from the FpsMode property
        public bool FpsMode { get { return _fpsMode; } set { throw new NotSupportedException(); } }
        private void updateGlfwMouseDisabled()
        {
            bool shouldBeDisabled = FpsOk && CaptureMouse; //both must be true: [window has focus] and [user wants mouse captured (ie user is not debugging)]
            if (shouldBeDisabled != FpsMode)
            {
                if (shouldBeDisabled)
                    _enableFPSMode();
                else
                    _disableFPSMode();
                _fpsMode = shouldBeDisabled; //remember computed value.  this is the only ok place to set this variable.
            }
        }
        /// <summary>
        /// Methods for toggling FPS mouse mode.
        /// </summary>
        private void _enableFPSMode()
        {
            MousePosInit();
            //Glfw.glfwDisable(Glfw.GLFW_MOUSE_CURSOR); //this doesn't always work.  bug in tao, can't be fixed.
        }
        private void _disableFPSMode()
        {
            //Glfw.glfwEnable(Glfw.GLFW_MOUSE_CURSOR);
        }



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
        /// Variables for GLFW callbacks.  Kept alive here so they don't get garbage collected (since we're using unmanaged code for this.)
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
        }

        /// <summary>
        /// Callback method for keyboard inputs
        /// </summary>
        /// <param name="key">Keyboard code</param>
        /// <param name="action">GLFW_PRESS or GLFW_RELEASE</param>
        void KeyboardInput(int temp, int action)
        {
            if (Program.norender > 0)
                return;

            bool pressed = (action == Glfw.GLFW_PRESS);
            GlfwKeys key = remap(temp);
            lock (Lock)
            {
                if (pressed)
                {
                    keys.Add(key);
                    downKeyEdges.Add(key);
                }
                else
                {
                    keys.Remove(key);
                    upKeyEdges.Add(key);
                }
            }

            //Program.WriteLine("key " + key.ToString() + (pressed ? " pressed." : " released."));

            if (pressed && key == GlfwKeys.GLFW_KEY_ESCAPE)
                CaptureMouse = !CaptureMouse; //toggle mouse capture

            lock (Lock)
                Monitor.PulseAll(Lock);
        }

        /// <summary>
        /// Callback method that updates the mouse delta
        /// </summary>
        /// <param name="x">position of mouse x position at callback</param>
        /// <param name="y">position of mouse y position at callback</param>
        void MousePos(int x, int y)
        {
            if (Program.norender > 0)
                return;

            if (FpsMode)
            {
                // Get difference of mouse pos & origin
                int tempX = originX - x;
                int tempY = originY - y;

                // add movement to rotation modified by sensitivity setting
                // positive X movement negatively rotates around Y axis
                rot.Y -= tempX * sens;

                if (!invertY)
                {
                    // positive Y movement negatively rotates around X axis with no invert
                    rot.X -= tempY * sens;
                }
                else
                {
                    rot.X += tempY * sens;
                }

                // Reset mouse to origin
                Glfw.glfwSetMousePos(originX, originY);
            }
            else
            {
                //Glfw.glfwEnable(Glfw.GLFW_MOUSE_CURSOR);
            }
            lock (Lock)
                Monitor.PulseAll(Lock);
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
            
            lock (Lock)
            {
                if (pressed)
                {
                    keys.Add(button);
                    downKeyEdges.Add(button);
                }
                else
                {
                    keys.Remove(button);
                    upKeyEdges.Add(button);
                }
            }

            lock (Lock)
                Monitor.PulseAll(Lock);
        }

        public void Clear()
        {
            rot.X = 0.0f;
            rot.Y = 0.0f;
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
        /// Public access methods for getting current rotation amount
        /// </summary>
        public float GetRotX()
        {
            return rot.X;
        }

        public float GetRotY()
        {
            return rot.Y;
        }

        /// <summary>
        /// More frequently called method for getting the rotation for the camera
        /// </summary>
        /// <param name="rotx"></param>
        /// <param name="roty"></param>
        public void GetRotAndClear(out float rotx, out float roty){
            rotx = rot.X;
            roty = rot.Y;

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

        HashSet<GlfwKeys> downKeyEdges = new HashSet<GlfwKeys>();
        HashSet<GlfwKeys> upKeyEdges = new HashSet<GlfwKeys>();
        public HashSet<GlfwKeys> GetDownKeyEdges()
        {
            Lock.AssertHeld();
            var r = downKeyEdges;
            downKeyEdges = new HashSet<GlfwKeys>();
            return r;
        }
        public HashSet<GlfwKeys> GetUpKeyEdges()
        {
            Lock.AssertHeld();
            var r = upKeyEdges;
            upKeyEdges = new HashSet<GlfwKeys>();
            return r;
        }
    }

    class KeyHappenedEventArgs : EventArgs
    {
        public GlfwKeys Key { get; set; }
        public bool Up { get; set; }
        public bool Down { get; set; }
    }
}
