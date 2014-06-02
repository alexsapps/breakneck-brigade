using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.FreeType;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class TextRenderer
    {
        private const int TEXTURE_WIDTH     = 256;
        private const int TEXTURE_HEIGHT    = 256;
        private const int GLYPH_WIDTH       = 16;
        private const int GLYPH_HEIGHT      = 16;
        private const int GLYPH_RENDER_WIDTH      = GLYPH_WIDTH-3;
        private const int COLS              = TEXTURE_WIDTH / GLYPH_WIDTH;
        private const int ROWS              = TEXTURE_HEIGHT / GLYPH_HEIGHT;

        private Texture FONT_TEXTURE  = Renderer.Textures[Renderer.FONT_TEXTURE];
        private Model     GLYPH_MODEL = Renderer.Models[Renderer.BLANKQUAD_MODEL_NAME];
        private Matrix4 DEFAULT_SCALE = Matrix4.MakeScalingMat(1f, 1f, 1f);

        private Dictionary<char, Model> _charToModel;

        public TextRenderer() 
        {
            _charToModel = new Dictionary<char, Model>();

            int currentIter = 0;
            int xInd, yInd;
            float xMin, xMax, yMin, yMax;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            

            for(char ii = 'A'; ii <= 'Z'; ii++)
            {
                _charToModel[ii] = makeCharacterModel(ii, xMin, xMax, yMin, yMax);
                currentIter++;
                updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            }
            for (char ii = 'a'; ii <= 'z'; ii++)
            {
                _charToModel[ii] = makeCharacterModel(ii, xMin, xMax, yMin, yMax);
                currentIter++;
                updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            }

            _charToModel['.'] = makeCharacterModel('.', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);

            _charToModel['!'] = makeCharacterModel('!', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);

            _charToModel['?'] = makeCharacterModel('?', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);

            for (char ii = '0'; ii <= '9'; ii++)
            {
                _charToModel[ii] = makeCharacterModel(ii, xMin, xMax, yMin, yMax);
                currentIter++;
                updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            }
            _charToModel['('] = makeCharacterModel('(', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel[')'] = makeCharacterModel(')', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel[':'] = makeCharacterModel(':', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel['-'] = makeCharacterModel('-', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel['\''] = makeCharacterModel('\'', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel['"'] = makeCharacterModel('"', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
            _charToModel[' '] = makeCharacterModel(' ', xMin, xMax, yMin, yMax);
            currentIter++;
            updateValues(currentIter, out xInd, out yInd, out xMin, out xMax, out yMin, out yMax);
        }

        /// <summary>
        /// Makes the necessary calls to render prims and draw the
        /// font textures on them.
        /// 
        /// Should only be called after a call to prep2D.
        /// The origin is the bottom left of the screen.
        /// </summary>
        /// <param name="x">The X location in pixels</param>
        /// <param name="y">The Y location in pixels</param>
        /// <param name="str"></param>
        public void printToScreen(float x, float y, string str, float scaleX, float scaleY)
        {
            for(int ii = 0; ii < str.Length; ii++)
            {
                char c = str[ii];
                float xPercent = (x + ii*GLYPH_RENDER_WIDTH) / (float) Renderer.WindowWidth;
                float yPercent = y / (float) Renderer.WindowHeight;
                Gl.glPushMatrix();

                Gl.glMultMatrixf(Matrix4.MakeTranslationMat(xPercent, yPercent, 0.0f).glArray);
                Gl.glMultMatrixf(Matrix4.MakeScalingMat(scaleX, scaleY, 1.0f).glArray);

                Model renderMe = _charToModel.ContainsKey(c) ? _charToModel[c] : _charToModel['?'];
                renderMe.Render();

                Gl.glPopMatrix();
            }
        }

        private Model makeCharacterModel(char c, float xMin, float xMax, float yMin, float yMax)
        {
            float [] T0 = {xMin, yMax};
            float [] T1 = {xMax, yMax};
            float [] T2 = {xMax, yMin};
            float [] T3 = {xMin, yMin};

            Model returnModel = new Model("character_" + c);
            VBO vbo = VBO.MakeQuadWithUVCoords(T0, T1, T2, T3);
            vbo.LoadData();
            TexturedMesh charMesh = new TexturedMesh() { VBO = vbo, Texture = Renderer.Textures["fontWhite.tga"] };
            returnModel.Meshes.Add(charMesh);

            return returnModel;
        }

        private void updateValues(int currentIter, out int xInd, out int yInd, out float xMin, out float xMax, out float yMin, out float yMax)
        {
            xInd = currentIter % COLS;
            yInd = currentIter / ROWS;
            xMin = xInd * GLYPH_WIDTH / (float)TEXTURE_WIDTH;
            xMax = (xInd + 1) * GLYPH_WIDTH / (float)TEXTURE_WIDTH;

            yMin = 1 - (yInd + 1) * GLYPH_HEIGHT / (float)TEXTURE_HEIGHT;
            yMax = 1 - yInd * GLYPH_HEIGHT / (float)TEXTURE_HEIGHT -0.002f;
        }
    }
}
