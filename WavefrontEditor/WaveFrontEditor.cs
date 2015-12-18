using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.IO;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.Enumerations;
using FileFormatWavefront;

namespace PolygonLoadingSample
{
    public partial class wfEditor : Form
    {
        //Объявление пременных, списков, камеры, и текстуры
        float CameraPosX = 0, CameraPosY = 0, CameraPosZ = -10, CameraPitch = 0, CameraYaw = 0, MouseHoldXold = 0, MouseHoldYold = 0;
        bool MouseHoldGL = false, ChooseBVert = false, ChooseBFace = false, FileLoadStat = false, USAStandart = false;
        int ChooseIndxVert = 0, ChooseIndxFace = 0, IncPointer;
        uint[] textures = new uint[1];
        Bitmap textureImage;
        List<string> FacesUIList = new List<string>();
        List<string> VerticesUIList = new List<string>();
        List<Polygon> polygons = new List<Polygon>();

        SharpGL.SceneGraph.Cameras.PerspectiveCamera camera = new SharpGL.SceneGraph.Cameras.PerspectiveCamera();
        public string Mtl { get; set; }

        public wfEditor() //инициализация окна 
        {
            InitializeComponent();

            //  Получить на OpenGL объект, для быстрого доступа.
            OpenGL gl = this.openGLControl1.OpenGL;

            //  A bit of extra initialisation here, we have to enable textures. Немного дополнительной инициализации здесь, мы должны включить текстуры.
            gl.Enable(OpenGL.GL_TEXTURE_2D);

            MouseHoldXold = openGLControl1.Width / 2;
            MouseHoldYold = openGLControl1.Height / 2;
        }

        //Метод для переопределения листбоксов и перезагрузки рендеринга
        public void Reint()
        {
            if (polygons.Count != 0)
            {
                VerticesUIList.Clear();
                IncPointer = 0;
                foreach (var vert in polygons[0].Vertices)
                    VerticesUIList.Add(String.Format(" {0} : ({1:0.00}, {2:0.00}, {3:0.00})", IncPointer++, vert.X, vert.Y, vert.Z));

                FacesUIList.Clear();
                IncPointer = 0;
                foreach (var face in polygons[0].Faces)
                    if (face.Count == 3)
                        FacesUIList.Add(" " + IncPointer++ + " : (" + face.Indices[0].Vertex + ", " + face.Indices[1].Vertex + ", " + face.Indices[2].Vertex + ")");
                    else if (face.Count == 4)
                        FacesUIList.Add(" " + IncPointer++ + " : (" + face.Indices[0].Vertex + ", " + face.Indices[1].Vertex + ", " + face.Indices[2].Vertex + ", " + face.Indices[3].Vertex + ")");
            }

            listBox1.DataSource = VerticesUIList;
            ((CurrencyManager)listBox1.BindingContext[listBox1.DataSource]).Refresh();
            listBox2.DataSource = FacesUIList;
            ((CurrencyManager)listBox2.BindingContext[listBox2.DataSource]).Refresh();

            foreach (var poly in polygons)
                poly.Unfreeze(openGLControl1.OpenGL);
            foreach (var poly in polygons)
                poly.Freeze(openGLControl1.OpenGL);
        }

        private void openGLControl1_OpenGLDraw(object sender, RenderEventArgs e) // 76 - 242 отрисовка опенГЛ
        {
            //  Get the OpenGL object, for quick access.
            var gl = this.openGLControl1.OpenGL;

            //  Clear and load the identity.
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.LoadIdentity();

            //  Вид с немного отошли оси Y и несколько блоков над землей.
            gl.LookAt(CameraPosX, CameraPosY, CameraPosZ, CameraPosX, CameraPosY, CameraPosZ + 1, 0, 1, 0);

            //  Вращать объекты, каждый цикл.
            gl.Rotate(CameraPitch, 1.0f, 0.0f, 0.0f);
            gl.Rotate(CameraYaw, 0.0f, 1.0f, 0.0f);
            gl.Rotate(0, 0.0f, 0.0f, 1.0f);

            //  Move the objects down a bit so that they fit in the screen better.
            gl.Translate(0, 0, -1);

            //Задаем цвет моделе
            gl.Color(1.0f, 1.0f, 1.0f);
            //Задаем текстуру моделе
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[0]);

            //Отрисовуем модель с 3х точечными и 3х точечными полигонами
            foreach (Polygon obj in polygons)
            {
                foreach (Face v in obj.Faces)
                {
                    if (v.Indices.Count == 3)
                    {
                        gl.Begin(OpenGL.GL_TRIANGLES);
                        gl.TexCoord(0, 0);
                        gl.Vertex(polygons[0].Vertices[v.Indices[0].Vertex].X, polygons[0].Vertices[v.Indices[0].Vertex].Y, polygons[0].Vertices[v.Indices[0].Vertex].Z);
                        gl.TexCoord(1, 0);
                        gl.Vertex(polygons[0].Vertices[v.Indices[1].Vertex].X, polygons[0].Vertices[v.Indices[1].Vertex].Y, polygons[0].Vertices[v.Indices[1].Vertex].Z);
                        gl.TexCoord(0, 1);
                        gl.Vertex(polygons[0].Vertices[v.Indices[2].Vertex].X, polygons[0].Vertices[v.Indices[2].Vertex].Y, polygons[0].Vertices[v.Indices[2].Vertex].Z);
                        gl.End();
                    }

                    if (v.Indices.Count == 4)
                    {
                        gl.Begin(OpenGL.GL_QUADS);
                        gl.TexCoord(0, 0);
                        gl.Vertex(polygons[0].Vertices[v.Indices[0].Vertex].X, polygons[0].Vertices[v.Indices[0].Vertex].Y, polygons[0].Vertices[v.Indices[0].Vertex].Z);
                        gl.TexCoord(1, 0);
                        gl.Vertex(polygons[0].Vertices[v.Indices[1].Vertex].X, polygons[0].Vertices[v.Indices[1].Vertex].Y, polygons[0].Vertices[v.Indices[1].Vertex].Z);
                        gl.TexCoord(1, 1);
                        gl.Vertex(polygons[0].Vertices[v.Indices[2].Vertex].X, polygons[0].Vertices[v.Indices[2].Vertex].Y, polygons[0].Vertices[v.Indices[2].Vertex].Z);
                        gl.TexCoord(0, 1);
                        gl.Vertex(polygons[0].Vertices[v.Indices[3].Vertex].X, polygons[0].Vertices[v.Indices[3].Vertex].Y, polygons[0].Vertices[v.Indices[3].Vertex].Z);
                        gl.End();
                    }
                }
            }

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE0);

            if (polygons.Count != 0)
                if (polygons[0].Vertices.Count == 0)
                    ChooseBVert = false;

            //Отрисовуем оси кординат при выборе точки
            if (ChooseBVert)
            {
                gl.Begin(OpenGL.GL_LINES);
                gl.Color(0.0f, 1.0f, 0.0f);                // Green for x axis
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X, polygons[0].Vertices[ChooseIndxVert].Y, polygons[0].Vertices[ChooseIndxVert].Z);
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X + 2, polygons[0].Vertices[ChooseIndxVert].Y, polygons[0].Vertices[ChooseIndxVert].Z);
                gl.Color(1.0f, 0.0f, 0.0f);                // Red for y axis
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X, polygons[0].Vertices[ChooseIndxVert].Y, polygons[0].Vertices[ChooseIndxVert].Z);
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X, polygons[0].Vertices[ChooseIndxVert].Y + 2, polygons[0].Vertices[ChooseIndxVert].Z);
                gl.Color(0.0f, 0.0f, 1.0f);                // Blue for z axis
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X, polygons[0].Vertices[ChooseIndxVert].Y, polygons[0].Vertices[ChooseIndxVert].Z);
                gl.Vertex(polygons[0].Vertices[ChooseIndxVert].X, polygons[0].Vertices[ChooseIndxVert].Y, polygons[0].Vertices[ChooseIndxVert].Z + 2);
                gl.End();

            }

            if (polygons.Count != 0)
                if (polygons[0].Faces.Count == 0)
                    ChooseBFace = false;

            //Отрисовуем обводку полигона при выборе полигона
            if (ChooseBFace)
            {
                if (polygons[0].Faces[ChooseIndxFace].Count == 3)
                {
                    int Xi = polygons[0].Faces[ChooseIndxFace].Indices[0].Vertex;
                    int Yi = polygons[0].Faces[ChooseIndxFace].Indices[1].Vertex;
                    int Zi = polygons[0].Faces[ChooseIndxFace].Indices[2].Vertex;

                    gl.Begin(OpenGL.GL_LINES);
                    gl.Color(1.0f, 1.0f, 0.0f);
                    gl.Vertex(polygons[0].Vertices[Xi].X, polygons[0].Vertices[Xi].Y, polygons[0].Vertices[Xi].Z);
                    gl.Vertex(polygons[0].Vertices[Yi].X, polygons[0].Vertices[Yi].Y, polygons[0].Vertices[Yi].Z);

                    gl.Vertex(polygons[0].Vertices[Yi].X, polygons[0].Vertices[Yi].Y, polygons[0].Vertices[Yi].Z);
                    gl.Vertex(polygons[0].Vertices[Zi].X, polygons[0].Vertices[Zi].Y, polygons[0].Vertices[Zi].Z);

                    gl.Vertex(polygons[0].Vertices[Xi].X, polygons[0].Vertices[Xi].Y, polygons[0].Vertices[Xi].Z);
                    gl.Vertex(polygons[0].Vertices[Zi].X, polygons[0].Vertices[Zi].Y, polygons[0].Vertices[Zi].Z);
                    gl.End();

                }

                if (polygons[0].Faces[ChooseIndxFace].Count == 4)
                {
                    int Xi = polygons[0].Faces[ChooseIndxFace].Indices[0].Vertex;
                    int Yi = polygons[0].Faces[ChooseIndxFace].Indices[1].Vertex;
                    int Zi = polygons[0].Faces[ChooseIndxFace].Indices[2].Vertex;
                    int Wi = polygons[0].Faces[ChooseIndxFace].Indices[3].Vertex;

                    gl.Begin(OpenGL.GL_LINES);
                    gl.Color(1.0f, 1.0f, 0.0f);
                    gl.Vertex(polygons[0].Vertices[Xi].X, polygons[0].Vertices[Xi].Y, polygons[0].Vertices[Xi].Z);
                    gl.Vertex(polygons[0].Vertices[Yi].X, polygons[0].Vertices[Yi].Y, polygons[0].Vertices[Yi].Z);

                    gl.Vertex(polygons[0].Vertices[Yi].X, polygons[0].Vertices[Yi].Y, polygons[0].Vertices[Yi].Z);
                    gl.Vertex(polygons[0].Vertices[Zi].X, polygons[0].Vertices[Zi].Y, polygons[0].Vertices[Zi].Z);

                    gl.Vertex(polygons[0].Vertices[Wi].X, polygons[0].Vertices[Wi].Y, polygons[0].Vertices[Wi].Z);
                    gl.Vertex(polygons[0].Vertices[Zi].X, polygons[0].Vertices[Zi].Y, polygons[0].Vertices[Zi].Z);

                    gl.Vertex(polygons[0].Vertices[Wi].X, polygons[0].Vertices[Wi].Y, polygons[0].Vertices[Wi].Z);
                    gl.Vertex(polygons[0].Vertices[Xi].X, polygons[0].Vertices[Xi].Y, polygons[0].Vertices[Xi].Z);
                    gl.End();
                }

            }

            if(file != null & FileLoadStat) // модель загружается в класс Polygon
//из которого потом отрисовывается
            {
                FileLoadStat = false;

                Polygon tmppoly = new Polygon();
                Vertex tmpPoint;
                Face tmpFace;

                foreach (var v in file.Model.Vertices)
                {
                    tmpPoint = new Vertex();
                    tmpPoint.X = v.x;
                    tmpPoint.Y = v.y;
                    tmpPoint.Z = v.z;

                    tmppoly.Vertices.Add(tmpPoint);
                }
                
                foreach (var f in file.Model.UngroupedFaces)
                {
                    tmpFace = new Face();
                    foreach (var i in f.Indices)
                    {
                        tmpFace.Indices.Add(new Index(i.vertex, -1, -1));
                    }

                    tmppoly.Faces.Add(tmpFace);
                }

                polygons.Add(tmppoly);

                Reint();
            }
        }

        FileLoadResult<FileFormatWavefront.Model.Scene> file;

        //Загрузка модели в редактор

        /// <summary>
        /// Handles the Click event of the importPolygonToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void importPolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Очищаем прошлую модель
            polygons.Clear();
            VerticesUIList.Clear();
            FacesUIList.Clear();

            ChooseBVert = false;
            ChooseBFace = false;

            Reint();

            //Загрузка файла
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Object files|*.obj";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                string sw = File.ReadAllText(openDialog.FileName);
                StreamWriter REFile = new StreamWriter(openDialog.FileName);
                REFile.Write(sw.Replace(USAStandart ? ',' : '.', USAStandart ? '.' : ','));
                REFile.Close();

                file = FileFormatObj.Load(openDialog.FileName, true);
                FileLoadStat = true;
            }
        }
        //Сохраняем модель в WaveFront(.obj)
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog openDialog = new SaveFileDialog();
            openDialog.Filter = "Object files|*.obj";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                StreamWriter sw = new StreamWriter(openDialog.FileName);

                StringBuilder Data = new StringBuilder();
                Data.Append("#WaveFrontEditor\n");
                Data.Append("o object\n");

                foreach (Vertex v in polygons[0].Vertices)
                {
                    Data.Append(String.Format("v {0} {1} {2}\n", v.X, v.Y, v.Z));
                }

                foreach (Face v in polygons[0].Faces)
                {
                    Data.Append("f");
                    for (int i = 0; i < v.Indices.Count; i++)
                        Data.Append(String.Format(" {0}//{1}", v.Indices[i].Vertex + 1, v.Indices[i].Normal + 1));
                    Data.Append("\n");
                }
                sw.Write(Data);
                sw.Close();
            }
        }


        /// <summary>
        /// Handles the Click event of the freezeAllToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void freezeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Остановка переотрисувки модели
            foreach (var poly in polygons)
                poly.Freeze(openGLControl1.OpenGL);
        }

        /// <summary>
        /// Handles the Click event of the unfreezeAllToolStripMenuItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void unfreezeAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Запуск преотрисовки модели
            foreach (var poly in polygons)
                poly.Unfreeze(openGLControl1.OpenGL);
        }

        //Первичные настройки рендеринга
        private void openGLControl1_OpenGLInitialized(object sender, EventArgs e)
        {
            openGLControl1.OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
        }

        //Настройка рендера Wireframe
        void WireframeToolStripMenuItemClick(object sender, EventArgs e)
        {
        	wireframeToolStripMenuItem.Checked = true;
        	solidToolStripMenuItem.Checked = false;
			lightedToolStripMenuItem.Checked = false;
        	openGLControl1.OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Lines);
        	openGLControl1.OpenGL.Disable(OpenGL.GL_LIGHTING);
        }

        //Настройка рендера Solid
        void SolidToolStripMenuItemClick(object sender, EventArgs e)
        {
        	wireframeToolStripMenuItem.Checked = false;
        	solidToolStripMenuItem.Checked = true;
        	lightedToolStripMenuItem.Checked = false;
        	openGLControl1.OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);        	
        	openGLControl1.OpenGL.Disable(OpenGL.GL_LIGHTING);
        }

        //Настройка рендера Lighted
        void LightedToolStripMenuItemClick(object sender, EventArgs e)
        {
        	wireframeToolStripMenuItem.Checked = false;
        	solidToolStripMenuItem.Checked = false;
        	lightedToolStripMenuItem.Checked = true;
        	openGLControl1.OpenGL.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);        	
        	openGLControl1.OpenGL.Enable(OpenGL.GL_LIGHTING);
        	openGLControl1.OpenGL.Enable(OpenGL.GL_LIGHT0);
        	openGLControl1.OpenGL.Enable(OpenGL.GL_COLOR_MATERIAL);
        	
        }
        //Выход
        void ExitToolStripMenuItemClick(object sender, EventArgs e)
        {
        	Close();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {
        }

        //Выбор точки в списке
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = polygons[0].Vertices[listBox1.SelectedIndex].X.ToString();
            textBox2.Text = polygons[0].Vertices[listBox1.SelectedIndex].Y.ToString();
            textBox3.Text = polygons[0].Vertices[listBox1.SelectedIndex].Z.ToString();

            ChooseBVert = true;
            ChooseIndxVert = listBox1.SelectedIndex;

            toolStripStatusLabel1.Text = String.Format("Vertices: {0} ", VerticesUIList[ChooseIndxVert]);
        }

        //Выбор полигона в списке
        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox4.Text = polygons[0].Faces[listBox2.SelectedIndex].Indices[0].Vertex.ToString();
            textBox5.Text = polygons[0].Faces[listBox2.SelectedIndex].Indices[1].Vertex.ToString();
            textBox6.Text = polygons[0].Faces[listBox2.SelectedIndex].Indices[2].Vertex.ToString();

            if (polygons[0].Faces[listBox2.SelectedIndex].Count == 3)
            {
                textBox7.Visible = false;
                label8.Visible = false;
            }
            if (polygons[0].Faces[listBox2.SelectedIndex].Count == 4)
            {
                label8.Visible = true;
                textBox7.Visible = true;
                textBox7.Text = polygons[0].Faces[listBox2.SelectedIndex].Indices[3].Vertex.ToString();
            }

            ChooseBFace = true;
            ChooseIndxFace = listBox2.SelectedIndex;
        }

        //Изменение кординат точки
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Vertex temp = polygons[0].Vertices[listBox1.SelectedIndex];
                temp.X = (float)Convert.ToDouble(textBox1.Text);
                temp.Y = (float)Convert.ToDouble(textBox2.Text);
                temp.Z = (float)Convert.ToDouble(textBox3.Text);
                polygons[0].Vertices[listBox1.SelectedIndex] = temp;

                Reint();
            }
            catch
            {
                MessageBox.Show("Invalid points.");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        //Изменение индексов полигона
        private void button5_Click(object sender, EventArgs e)
        {

            try
            {
                polygons[0].Faces[listBox2.SelectedIndex].Indices[0] = new Index(Convert.ToInt32(textBox4.Text), -1, -1);
                polygons[0].Faces[listBox2.SelectedIndex].Indices[1] = new Index(Convert.ToInt32(textBox5.Text), -1, -1);
                polygons[0].Faces[listBox2.SelectedIndex].Indices[2] = new Index(Convert.ToInt32(textBox6.Text), -1, -1);

                //Если в полигоне 4 точки
                if (polygons[0].Faces[ChooseIndxFace].Count == 4)
                {
                    polygons[0].Faces[listBox2.SelectedIndex].Indices[3] = new Index(Convert.ToInt32(textBox7.Text), -1, -1);
                }

                Reint();
            }
            catch
            {
                MessageBox.Show("Invalid points.");
            }
        }

        //Добавление нового полигона на 3 точки
        private void button6_Click(object sender, EventArgs e)
        {

                if (polygons.Count == 0)
                    polygons.Add(new Polygon());
            if (polygons[0].Vertices.Count != 0)
            {
                Face temp = new Face();

                Index tempcord = new Index(0, -1, -1);
                temp.Indices.Add(tempcord);
                temp.Indices.Add(tempcord);
                temp.Indices.Add(tempcord);

                polygons[0].Faces.Add(temp);

                Reint();
            }
            else
            {
                MessageBox.Show("Not enough points.");
            }
        }

        //Добавление нового полигона на 4 точки
        private void button8_Click(object sender, EventArgs e)
        {

                if (polygons.Count == 0)
                    polygons.Add(new Polygon());
            if (polygons[0].Vertices.Count != 0)
            {
                Face temp = new Face();

                Index tempcord = new Index(0, -1, -1);
                temp.Indices.Add(tempcord);
                temp.Indices.Add(tempcord);
                temp.Indices.Add(tempcord);
                temp.Indices.Add(tempcord);

                polygons[0].Faces.Add(temp);

                Reint();
            }
            else
            {
                MessageBox.Show("Not enough points.");
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            USAStandart = checkBox1.Checked;
        }

        //Удаление выделеного полигона
        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                polygons[0].Faces.Remove(polygons[0].Faces[ChooseIndxFace]);

                Reint();
            }
            catch
            {
                MessageBox.Show("Error");
            }
        }

        //Загрузка текстуры
        private void addTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files|*.bmp;*.jpg;*.jpeg|All Files|*.*";
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                SharpGL.OpenGL gl = this.openGLControl1.OpenGL;

                textureImage = new Bitmap(openDialog.FileName);
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                gl.GenTextures(1, textures);
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, textures[0]);

                gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, 3, textureImage.Width, textureImage.Height, 0, OpenGL.GL_BGR, OpenGL.GL_UNSIGNED_BYTE,
                    textureImage.LockBits(new Rectangle(0, 0, textureImage.Width, textureImage.Height),
                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb).Scan0);

                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR);
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
        }

        //Добавление новой точки
        private void button2_Click(object sender, EventArgs e)
        {
            if (polygons.Count == 0)
                polygons.Add(new Polygon());
            Vertex temp = new Vertex();
            temp.X = 0; temp.Y = 0; temp.Z = 0;

            polygons[0].Vertices.Add(temp);

            Reint();
        }

        //Захват миши по окну рендеринга
        private void openGLControl1_MouseDown(object sender, MouseEventArgs e)
        {
            MouseHoldGL = true;
        }
        private void openGLControl1_MouseUp(object sender, MouseEventArgs e)
        {
            MouseHoldGL = false;
        }

        private void openGLControl1_MouseClick(object sender, MouseEventArgs e)
        {
        }

        //Управление камерой Левая кнопка - приблизить, отдалить. Средняя - переместить камеру. Правая - изменить угол камеры.
        private void openGLControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MouseHoldGL & e.Button.ToString() == "Left")
            {
                CameraPosZ += (MouseHoldYold - e.Y) / 100;
            }
            if (MouseHoldGL & e.Button.ToString() == "Middle")
            {
                CameraPosX -= (MouseHoldXold - e.X) / 100;
                CameraPosY -= (MouseHoldYold - e.Y) / 100;
            }
            if (MouseHoldGL & e.Button.ToString() == "Right")
            {
                CameraYaw -= (MouseHoldXold - e.X) / 2;
                CameraPitch += (MouseHoldYold - e.Y) / 2;
            }
            MouseHoldXold = e.X;
            MouseHoldYold = e.Y;
        }

        //Новая сцена
        void ClearToolStripMenuItemClick(object sender, EventArgs e)
        {
        	polygons.Clear();
            VerticesUIList.Clear();
            FacesUIList.Clear();

            ChooseBVert = false;
            ChooseBFace = false;

            Reint();
        }

        private void openGLControl1_Load(object sender, EventArgs e)
        {
        }
    }
}