using System;
using System.Drawing;
using System.Windows.Media.Media3D;

namespace RenderEngine
{
    internal class SkyboxRender
    {
        void RenderSkybox(WorldCamera camera, Skybox skybox, int rows, int cols)
        {
            // Loop through walls and layers
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (IsCellVisible(camera, r, c))
                    {
                        RenderCell(skybox.NorthWall[r, c], "North");
                        RenderCell(skybox.SouthWall[r, c], "South");
                        RenderCell(skybox.EastWall[r, c], "East");
                        RenderCell(skybox.WestWall[r, c], "West");
                        RenderCell(skybox.TopLayer[r, c], "Top");
                        RenderCell(skybox.BottomLayer[r, c], "Bottom");
                    }
                }
            }
        }

        private void RenderCell(string v1, string v2)
        {
            throw new NotImplementedException();
        }

        bool IsCellVisible(WorldCamera camera, int row, int col)
        {
            // Perform visibility check based on the camera's FOV and cell position
            return true; // Placeholder logic
        }

        //TODO pseudo code

        //Bitmap RenderSkybox(Camera camera, Skybox skybox, int width, int height)
        //{
        //    Bitmap frame = new Bitmap(width, height);

        //    using (Graphics g = Graphics.FromImage(frame))
        //    {
        //        g.Clear(Color.Black); // Clear background

        //        foreach (var cell in skybox.GetVisibleCells(camera))
        //        {
        //            // Transform and project the cell into screen space
        //            var screenRect = ProjectCellToScreen(cell, camera, width, height);

        //            // Draw the cell's texture, blending edges if necessary
        //            g.DrawImage(cell.Texture, screenRect);
        //        }
        //    }

        //    return frame;
        //}

        //Bitmap CompositeFrame(Camera camera, Raycaster raycaster, Skybox skybox, int width, int height)
        //{
        //    // Render the skybox
        //    Bitmap skyboxFrame = RenderSkybox(camera, skybox, width, height);

        //    // Render the raycaster's output
        //    Bitmap raycasterFrame = raycaster.Render(camera, width, height);

        //    // Combine the two frames
        //    Bitmap finalFrame = new Bitmap(width, height);
        //    using (Graphics g = Graphics.FromImage(finalFrame))
        //    {
        //        g.DrawImage(skyboxFrame, 0, 0);       // Draw skybox first
        //        g.DrawImage(raycasterFrame, 0, 0);   // Overlay raycaster output
        //    }

        //    return finalFrame;
        //}

    }
}
