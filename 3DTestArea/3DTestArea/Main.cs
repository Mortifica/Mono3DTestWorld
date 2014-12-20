using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace _3DTestArea
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Game
    {
         public struct VertexPositionColorNormal : IVertexType
         {
             public Vector3 Position;
             public Color Color;
             public Vector3 Normal;
 
             public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
             (
                 new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                 new VertexElement(sizeof(float) * 3, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                 new VertexElement(sizeof(float) * 3 + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
             );

             VertexDeclaration IVertexType.VertexDeclaration
             {
                 get { throw new System.NotImplementedException(); }
             }
         }
 
        GraphicsDeviceManager graphics;
        GraphicsDevice device;
        SpriteBatch spriteBatch;

        Effect effects;
        VertexPositionColorNormal[] vertices;
        Matrix viewMatrix;
        Matrix projectionMatrix;
        int[] indices;
 
        private float angle = 0f;
        private int terrainWidth = 4;
        private int terrainHeight = 3;
        private float[,] heightData;
        public Main()
            : base()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.PreferredBackBufferWidth = 500;
            graphics.PreferredBackBufferHeight = 500;
            graphics.IsFullScreen = false;
            graphics.ApplyChanges();
            Window.Title = "Riemer's XNA Tutorials -- 3D Series 1";
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            device = graphics.GraphicsDevice;
            //code here is to make xna custom fx work with monogame fx
            byte[] bytecode = File.ReadAllBytes("Content/Effects/effects.mgfx");
            effects = new Effect(device, bytecode);

            SetUpCamera();
            Texture2D heightMap = Content.Load<Texture2D>("Terrain/heightmap");
            LoadHeightData(heightMap);
            SetUpVertices();
            SetUpIndices();
            CalculateNormals();
            // TODO: use this.Content to load your game content here
        }

protected override void UnloadContent()
         {
         }
 
         private void SetUpVertices()
         {
             float minHeight = float.MaxValue;
             float maxHeight = float.MinValue;
             for (int x = 0; x < terrainWidth; x++)
             {
                 for (int y = 0; y < terrainHeight; y++)
                 {
                     if (heightData[x, y] < minHeight)
                         minHeight = heightData[x, y];
                     if (heightData[x, y] > maxHeight)
                         maxHeight = heightData[x, y];
                 }
             }
 
             vertices = new VertexPositionColorNormal[terrainWidth * terrainHeight];
             for (int x = 0; x < terrainWidth; x++)
             {
                 for (int y = 0; y < terrainHeight; y++)
                 {
                     vertices[x + y * terrainWidth].Position = new Vector3(x, heightData[x, y], -y);
 
                     if (heightData[x, y] < minHeight + (maxHeight - minHeight) / 4)
                         vertices[x + y * terrainWidth].Color = Color.Blue;
                     else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 2 / 4)
                         vertices[x + y * terrainWidth].Color = Color.Green;
                     else if (heightData[x, y] < minHeight + (maxHeight - minHeight) * 3 / 4)
                         vertices[x + y * terrainWidth].Color = Color.Brown;
                     else
                         vertices[x + y * terrainWidth].Color = Color.White;
                 }
             }
         }
 
         private void SetUpIndices()
         {
             indices = new int[(terrainWidth - 1) * (terrainHeight - 1) * 6];
             int counter = 0;
             for (int y = 0; y < terrainHeight - 1; y++)
             {
                 for (int x = 0; x < terrainWidth - 1; x++)
                 {
                     int lowerLeft = x + y * terrainWidth;
                     int lowerRight = (x + 1) + y * terrainWidth;
                     int topLeft = x + (y + 1) * terrainWidth;
                     int topRight = (x + 1) + (y + 1) * terrainWidth;
 
                     indices[counter++] = topLeft;
                     indices[counter++] = lowerRight;
                     indices[counter++] = lowerLeft;
 
                     indices[counter++] = topLeft;
                     indices[counter++] = topRight;
                     indices[counter++] = lowerRight;
                 }
             }
         }
 
         private void CalculateNormals()
         {
             for (int i = 0; i < vertices.Length; i++)
                 vertices[i].Normal = new Vector3(0, 0, 0);
 
             for (int i = 0; i < indices.Length / 3; i++)
             {
                 int index1 = indices[i * 3];
                 int index2 = indices[i * 3 + 1];
                 int index3 = indices[i * 3 + 2];
 
                 Vector3 side1 = vertices[index1].Position - vertices[index3].Position;
                 Vector3 side2 = vertices[index1].Position - vertices[index2].Position;
                 Vector3 normal = Vector3.Cross(side1, side2);
 
                 vertices[index1].Normal += normal;
                 vertices[index2].Normal += normal;
                 vertices[index3].Normal += normal;
             }
 
             for (int i = 0; i < vertices.Length; i++)
                 vertices[i].Normal.Normalize();
         }
 
         private void LoadHeightData(Texture2D heightMap)
         {
             terrainWidth = heightMap.Width;
             terrainHeight = heightMap.Height;
 
             Color[] heightMapColors = new Color[terrainWidth * terrainHeight];
             heightMap.GetData(heightMapColors);
 
             heightData = new float[terrainWidth, terrainHeight];
             for (int x = 0; x < terrainWidth; x++)
                 for (int y = 0; y < terrainHeight; y++)
                     heightData[x, y] = heightMapColors[x + y * terrainWidth].R / 5.0f;
         }
 
         private void SetUpCamera()
         {
             viewMatrix = Matrix.CreateLookAt(new Vector3(60, 80, -80), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
             projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1.0f, 300.0f);
         }
 
         protected override void Update(GameTime gameTime)
         {
             if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                 this.Exit();
 
             KeyboardState keyState = Keyboard.GetState();
             if (keyState.IsKeyDown(Keys.E))
                 angle += 0.05f;
             if (keyState.IsKeyDown(Keys.D))
                 angle -= 0.05f;
 
             base.Update(gameTime);
         }
 
         protected override void Draw(GameTime gameTime)
         {
             device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
 
             RasterizerState rs = new RasterizerState();
             rs.CullMode = CullMode.None;
             device.RasterizerState = rs;
 
             Matrix worldMatrix = Matrix.CreateTranslation(-terrainWidth / 2.0f, 0, terrainHeight / 2.0f) * Matrix.CreateRotationY(angle);
             effects.CurrentTechnique = effects.Techniques["Colored"];
             effects.Parameters["xView"].SetValue(viewMatrix);
             effects.Parameters["xProjection"].SetValue(projectionMatrix);
             effects.Parameters["xWorld"].SetValue(worldMatrix);
             Vector3 lightDirection = new Vector3(1.0f, -1.0f, -1.0f);
             lightDirection.Normalize();
             effects.Parameters["xLightDirection"].SetValue(lightDirection);
             effects.Parameters["xAmbient"].SetValue(0.1f);
             effects.Parameters["xEnableLighting"].SetValue(true);            
 
             foreach (EffectPass pass in effects.CurrentTechnique.Passes)
             {
                 pass.Apply();
 
                 device.DrawUserIndexedPrimitives<VertexPositionColorNormal>(PrimitiveType.TriangleList, vertices, 0, vertices.Length, indices, 0, indices.Length / 3, VertexPositionColorNormal.VertexDeclaration);
             }
 
             base.Draw(gameTime);
         
        }
    }
}
