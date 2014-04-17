using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Glfw;
using SousChef;

namespace Breakneck_Brigade
{
    class InputManager
    {
        static const int GLFW_KEY_UNKNOWN       = -1; 
        static const int GLFW_KEY_SPACE         = 32;
        static const int GLFW_KEY_APOSTROPHE    = 39; // ' 
        static const int GLFW_KEY_COMMA         = 44; // , 
        static const int GLFW_KEY_MINUS         = 45; // - 
        static const int GLFW_KEY_PERIOD        = 46; // . 
        static const int GLFW_KEY_SLASH         = 47; // / 
        static const int GLFW_KEY_0             = 48;
        static const int GLFW_KEY_1             = 49;
        static const int GLFW_KEY_2             = 50;
        static const int GLFW_KEY_3             = 51;
        static const int GLFW_KEY_4             = 52;
        static const int GLFW_KEY_5             = 53;
        static const int GLFW_KEY_6             = 54;
        static const int GLFW_KEY_7             = 55;
        static const int GLFW_KEY_8             = 56;
        static const int GLFW_KEY_9             = 57;
        static const int GLFW_KEY_SEMICOLON     = 59; // ; 
        static const int GLFW_KEY_EQUAL         = 61; // = 
        static const int GLFW_KEY_A             = 65;
        static const int GLFW_KEY_B             = 66;
        static const int GLFW_KEY_C             = 67;
        static const int GLFW_KEY_D             = 68;
        static const int GLFW_KEY_E             = 69;
        static const int GLFW_KEY_F             = 70;
        static const int GLFW_KEY_G             = 71;
        static const int GLFW_KEY_H             = 72;
        static const int GLFW_KEY_I             = 73;
        static const int GLFW_KEY_J             = 74;
        static const int GLFW_KEY_K             = 75;
        static const int GLFW_KEY_L             = 76;
        static const int GLFW_KEY_M             = 77;
        static const int GLFW_KEY_N             = 78;
        static const int GLFW_KEY_O             = 79;
        static const int GLFW_KEY_P             = 80;
        static const int GLFW_KEY_Q             = 81;
        static const int GLFW_KEY_R             = 82;
        static const int GLFW_KEY_S             = 83;
        static const int GLFW_KEY_T             = 84;
        static const int GLFW_KEY_U             = 85;
        static const int GLFW_KEY_V             = 86;
        static const int GLFW_KEY_W             = 87;
        static const int GLFW_KEY_X             = 88;
        static const int GLFW_KEY_Y             = 89;
        static const int GLFW_KEY_Z             = 90;
        static const int GLFW_KEY_LEFT_BRACKET  = 91; // [
        static const int GLFW_KEY_BACKSLASH     = 92; // \
        static const int GLFW_KEY_RIGHT_BRACKET = 93; // ]
        static const int GLFW_KEY_GRAVE_ACCENT  = 96; // `
        static const int GLFW_KEY_WORLD_1       = 161; // non-US #1
        static const int GLFW_KEY_WORLD_2       = 162; // non-US #2
        static const int GLFW_KEY_ESCAPE        = 256;
        static const int GLFW_KEY_ENTER         = 257;
        static const int GLFW_KEY_TAB           = 258;
        static const int GLFW_KEY_BACKSPACE     = 259;
        static const int GLFW_KEY_INSERT        = 260;
        static const int GLFW_KEY_DELETE        = 261;
        static const int GLFW_KEY_RIGHT         = 262;
        static const int GLFW_KEY_LEFT          = 263;
        static const int GLFW_KEY_DOWN          = 264;
        static const int GLFW_KEY_UP            = 265;
        static const int GLFW_KEY_PAGE_UP       = 266;
        static const int GLFW_KEY_PAGE_DOWN     = 267;
        static const int GLFW_KEY_HOME          = 268;
        static const int GLFW_KEY_END           = 269;
        static const int GLFW_KEY_CAPS_LOCK     = 280;
        static const int GLFW_KEY_SCROLL_LOCK   = 281;
        static const int GLFW_KEY_NUM_LOCK      = 282;
        static const int GLFW_KEY_PRINT_SCREEN  = 283;
        static const int GLFW_KEY_PAUSE         = 284;
        static const int GLFW_KEY_F1            = 290;
        static const int GLFW_KEY_F2            = 291;
        static const int GLFW_KEY_F3            = 292;
        static const int GLFW_KEY_F4            = 293;
        static const int GLFW_KEY_F5            = 294;
        static const int GLFW_KEY_F6            = 295;
        static const int GLFW_KEY_F7            = 296;
        static const int GLFW_KEY_F8            = 297;
        static const int GLFW_KEY_F9            = 298;
        static const int GLFW_KEY_F10           = 299;
        static const int GLFW_KEY_F11           = 300;
        static const int GLFW_KEY_F12           = 301;
        static const int GLFW_KEY_F13           = 302;
        static const int GLFW_KEY_F14           = 303;
        static const int GLFW_KEY_F15           = 304;
        static const int GLFW_KEY_F16           = 305;
        static const int GLFW_KEY_F17           = 306;
        static const int GLFW_KEY_F18           = 307;
        static const int GLFW_KEY_F19           = 308;
        static const int GLFW_KEY_F20           = 309;
        static const int GLFW_KEY_F21           = 310;
        static const int GLFW_KEY_F22           = 311;
        static const int GLFW_KEY_F23           = 312;
        static const int GLFW_KEY_F24           = 313;
        static const int GLFW_KEY_F25           = 314;
        static const int GLFW_KEY_KP_0          = 320;
        static const int GLFW_KEY_KP_1          = 321;
        static const int GLFW_KEY_KP_2          = 322;
        static const int GLFW_KEY_KP_3          = 323;
        static const int GLFW_KEY_KP_4          = 324;
        static const int GLFW_KEY_KP_5          = 325;
        static const int GLFW_KEY_KP_6          = 326;
        static const int GLFW_KEY_KP_7          = 327;
        static const int GLFW_KEY_KP_8          = 328;
        static const int GLFW_KEY_KP_9          = 329;
        static const int GLFW_KEY_KP_DECIMAL    = 330;
        static const int GLFW_KEY_KP_DIVIDE     = 331;
        static const int GLFW_KEY_KP_MULTIPLY   = 332;
        static const int GLFW_KEY_KP_SUBTRACT   = 333;
        static const int GLFW_KEY_KP_ADD        = 334;
        static const int GLFW_KEY_KP_ENTER      = 335;
        static const int GLFW_KEY_KP_EQUAL      = 336;
        static const int GLFW_KEY_LEFT_SHIFT    = 340;
        static const int GLFW_KEY_LEFT_CONTROL  = 341;
        static const int GLFW_KEY_LEFT_ALT      = 342;
        static const int GLFW_KEY_LEFT_SUPER    = 343;
        static const int GLFW_KEY_RIGHT_SHIFT   = 344;
        static const int GLFW_KEY_RIGHT_CONTROL = 345;
        static const int GLFW_KEY_RIGHT_ALT     = 346;
        static const int GLFW_KEY_RIGHT_SUPER   = 347;
        static const int GLFW_KEY_MENU          = 348;
        static const int GLFW_KEY_LAST          = GLFW_KEY_MENU;

        static const int GLFW_MOUSE_BUTTON_1    = 0;
        static const int GLFW_MOUSE_BUTTON_2    = 1;
        static const int GLFW_MOUSE_BUTTON_LEFT = GLFW_MOUSE_BUTTON_1;
        static const int GLFW_MOUSE_BUTTON_RIGHT = GLFW_MOUSE_BUTTON_2;

        GlobalsConfigFile globalConfig;
        Hashtable keys = new Hashtable();

        // Game-related stuff
        bool fpsMode = false;        
        int originX = 0;
        int originY = 0;
        Vector4 rot = new Vector4();
        float sens = 1.0f;
        bool invertY = false;

        public InputManager()
        {
            // callback for keyboard; will trigger KeyboardInput() on every keypress or release
            Glfw.glfwSetKeyCallback(KeyboardInput);
            // callback for mouse; will trigger MousePos() on every mouse movement
            Glfw.glfwSetMousePosCallback(MousePos);
            // callback for mouse; will trigger MouseButton() on every mouse click/release
            Glfw.glfwSetMouseButtonCallback(MouseButton);

            globalConfig = new GlobalsConfigFolder(BBXml.GetLocalConfigFolder()).Open("keys.xml");

            keys[GLFW_KEY_W] = 0;
            keys[GLFW_KEY_A] = 0;
            keys[GLFW_KEY_S] = 0;
            keys[GLFW_KEY_D] = 0;
            keys[GLFW_KEY_SPACE] = 0;
            keys[GLFW_KEY_ESCAPE] = 0;

            keys[GLFW_MOUSE_BUTTON_LEFT] = 0;
            keys[GLFW_MOUSE_BUTTON_RIGHT] = 0;
        }

        void KeyboardInput(int key, int action)
        {
            bool pressed = action == Glfw.GLFW_PRESS ? true : false;
            int temp = remap(key);
            keys[temp] = pressed;
        }

        void MousePos(int x, int y)
        {
            if (fpsMode)
            {
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

                // constrain rotation amounts
                // To be implemented; radian or degrees?

                // Reset mouse to origin
                Glfw.glfwSetMousePos(originX, originY);
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
            // To-be-implemented; get invertY from config
            // To-be-implemented; swap left/right click
            /* if(swapFlag)
             * {
             *      mouse button left = mouse button 2;
             *      mouse button right = mouse button 1;
             * }
             */
        }

        void MouseButton(int button, int action)
        {
            bool pressed = action == Glfw.GLFW_PRESS ? true : false;
            keys[button] = pressed;
        }

        /// <summary>
        /// Remaps rebound keys from config file to locally defined values.
        /// </summary>
        /// <param name="key">value of key passed to callback method</param>
        /// <returns>value of key as defined by config file or default value if not defined</returns>
        private int remap(int key)
        {
            // if config load is broken,
            // return key;
            return int.Parse(globalConfig.GetSetting(key.ToString(), key));
        }

        /// <summary>
        /// Methods for toggling FPS mouse mode.
        /// </summary>
        void TurnOnFPSMode()
        {
            fpsMode = true;
        }

        void TurnOffFPSMode()
        {
            fpsMode = false;
        }
    }
}
