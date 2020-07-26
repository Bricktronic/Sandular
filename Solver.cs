﻿using SFML.System;
using System;
using System.Collections.Generic;

namespace Sandular
{
    class Solver
    {
        public static List<PowderPoint> Particles;
        public static Dictionary<Vector2u, GridPoint> GridPoints;
        public static List<Vector2u> RemoveFromGrid = new List<Vector2u>();
        public static List<PowderPoint> RemoveFromParticles = new List<PowderPoint>();
        public static bool VoidMode = true;
        public static float Gravity = 9.8f;
        public static uint GridSize = 15;
        public static Random RDM;

        public static void Tick()
        {
            SimulateGrids();
            RemoveClearedParticles();
            ClearGridParticles();
            AssertAndCheck();
            ClearEmptyGrids();

            RDM = new Random();
        }

        public static void RemoveClearedParticles()
        {
            foreach (PowderPoint Particle in RemoveFromParticles)
            {
                Particles.Remove(Particle);
            }
            RemoveFromParticles.Clear();
        }

        public static void ClearEmptyGrids()
        {
            foreach (KeyValuePair<Vector2u, GridPoint> Entry in GridPoints)
            {
                if (Entry.Value.Particles.Count == 0)
                {
                    RemoveFromGrid.Add(Entry.Key);
                }
            }

            foreach (Vector2u Remove in RemoveFromGrid)
            {
                GridPoints.Remove(Remove);
            }
            RemoveFromGrid.Clear();
        }

        public static void AssertAndCheck()
        {
            for (int i = 0; i < Particles.Count; i++)
            {
                if ((uint)Particles[i].PX >= Program.Resolution.X - 1)
                    if (VoidMode == true)
                    {
                        Particles.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        Particles[i].PX = 1;
                    }

                if ((uint)Particles[i].PY >= Program.Resolution.Y - 1)
                    if (VoidMode == true)
                    {
                        Particles.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        Particles[i].PY = 0;
                    }

                if ((uint)Particles[i].PX < 0)
                    if (VoidMode == true)
                    {
                        Particles.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        Particles[i].PX = Program.Resolution.X - 1;
                    }

                if ((uint)Particles[i].PY < 0)
                    if (VoidMode == true)
                    {
                        Particles.RemoveAt(i);
                        continue;
                    }
                    else
                    {
                        Particles[i].PY = Program.Resolution.Y - 1;
                    }

                AssertParticle(i);
            }
        }

        public static void SimulateGrids()
        {
            foreach (KeyValuePair<Vector2u, GridPoint> Entry in GridPoints)
            {
                foreach (int i in Entry.Value.Particles.Values)
                {
                    if (Resources.PowderTypes[(int)Particles[i].Type].Drag > 0)
                    {
                        //Gravitational pull
                        Particles[i].VY += Gravity * 0.01f;

                        //Drag implementation
                        Particles[i].VX *= 1 / (1 + Resources.PowderTypes[(int)Particles[i].Type].Drag);
                        Particles[i].VY *= 1 / (1 + Resources.PowderTypes[(int)Particles[i].Type].Drag);

                        //Get neighbor data
                        int[] Neighbors = GetNeighbors((uint)Math.Round(Particles[i].PX), (uint)Math.Round(Particles[i].PY));

                        //Liquid movement
                        if (Resources.PowderTypes[(int)Particles[i].Type].Liquid)
                        {
                            Particles[i].VX += RDM.Next(-1, 2)*0.1f;
                            Particles[i].VY += RDM.Next(-1,2)*0.1f;
                        }

                        //UP neighbor
                        if (Neighbors[0] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VY < 0 && Particles[Neighbors[0]].VY > Particles[i].VY)
                            {
                                /*if(Resources.PowderTypes[(int)Particles[Neighbors[1]].Type].Drag > 0 || Resources.PowderTypes[(int)Particles[i].Type].Mass > Resources.PowderTypes[(int)Particles[Neighbors[0]].Type].Mass)
                                {
                                    float Old = Particles[0].PY;
                                    Particles[i].PY = Particles[Neighbors[0]].PY;
                                    Particles[Neighbors[0]].PY = Old;
                                    continue;
                                }*/

                                bool RSide = false;
                                bool LSide = false;

                                //UP LEFT slide check
                                if (Neighbors[4] == -1)
                                    LSide = true;

                                //UP RIGHT slide check
                                if (Neighbors[5] == -1)
                                    RSide = true;

                                if (RSide && LSide)
                                {
                                    switch (RDM.Next(0, 2))
                                    {
                                        case 0:
                                            Particles[i].VX += 0.1f;
                                            break;
                                        case 1:
                                            Particles[i].VX -= 0.1f;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (RSide)
                                    {
                                        Particles[i].VX += 0.1f;
                                    }

                                    if (LSide)
                                    {
                                        Particles[i].VX -= 0.1f;
                                    }
                                }
                            }
                        }

                        //DOWN neighbor
                        if (Neighbors[1] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VY > 0 && Particles[Neighbors[1]].VY < Particles[i].VY)
                            {
                                bool RSide = false;
                                bool LSide = false;

                                //DOWN LEFT slide check
                                if (Neighbors[6] == -1)
                                    LSide = true;

                                //DOWN RIGHT slide check
                                if (Neighbors[7] == -1)
                                    RSide = true;

                                if (RSide && LSide)
                                {
                                    switch (RDM.Next(0, 2))
                                    {
                                        case 0:
                                            Particles[i].VX += 0.1f;
                                            break;
                                        case 1:
                                            Particles[i].VX -= 0.1f;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (RSide)
                                    {
                                        Particles[i].VX += 0.1f;
                                    }

                                    if (LSide)
                                    {
                                        Particles[i].VX -= 0.1f;
                                    }
                                }
                            }
                        }

                        //RIGHT neighbor
                        if (Neighbors[3] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VX > 0 && Particles[Neighbors[3]].VX < Particles[i].VX)
                            {
                                bool USide = false;
                                bool DSide = false;

                                //UP slide check
                                if (Neighbors[5] == -1)
                                    USide = true;

                                //DOWN slide check
                                if (Neighbors[7] == -1)
                                    DSide = true;

                                if (DSide && USide)
                                {
                                    switch (RDM.Next(0, 2))
                                    {
                                        case 0:
                                            Particles[i].VY += 0.1f;
                                            break;
                                        case 1:
                                            Particles[i].VY -= 0.1f;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (DSide)
                                    {
                                        Particles[i].VY += 0.1f;
                                    }

                                    if (USide)
                                    {
                                        Particles[i].VY -= 0.1f;
                                    }
                                }
                            }
                        }

                        //LEFT neighbor
                        if (Neighbors[2] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VX < 0 && Particles[Neighbors[2]].VX > Particles[i].VX)
                            {
                                bool USide = false;
                                bool DSide = false;

                                //UP slide check
                                if (Neighbors[4] == -1)
                                    USide = true;

                                //DOWN slide check
                                if (Neighbors[6] == -1)
                                    DSide = true;

                                if (DSide && USide)
                                {
                                    switch (RDM.Next(0, 2))
                                    {
                                        case 0:
                                            Particles[i].VY += 0.1f;
                                            break;
                                        case 1:
                                            Particles[i].VY -= 0.1f;
                                            break;
                                    }
                                }
                                else
                                {
                                    if (DSide)
                                    {
                                        Particles[i].VY += 0.1f;
                                    }

                                    if (USide)
                                    {
                                        Particles[i].VY -= 0.1f;
                                    }
                                }
                            }
                        }

                        //UP neighbor
                        if (Neighbors[0] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VY < 0 && Particles[Neighbors[0]].VY > Particles[i].VY)
                            {
                                Particles[i].VY = Particles[Neighbors[0]].VY;
                            }
                        }

                        //DOWN neighbor
                        if (Neighbors[1] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VY > 0 && Particles[Neighbors[1]].VY < Particles[i].VY)
                            {
                                Particles[i].VY = Particles[Neighbors[1]].VY;
                            }
                        }

                        //RIGHT neighbor
                        if (Neighbors[3] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VX > 0 && Particles[Neighbors[3]].VX < Particles[i].VX)
                            {
                                Particles[i].VX = Particles[Neighbors[3]].VX;
                            }
                        }

                        //LEFT neighbor
                        if (Neighbors[2] != -1)
                        {
                            //Force Transferral
                            if (Particles[i].VX < 0 && Particles[Neighbors[2]].VX > Particles[i].VX)
                            {
                                Particles[i].VX = Particles[Neighbors[2]].VX;
                            }
                        }

                        if (Particles[i].VX > 0.99f)
                            Particles[i].VX = 0.99f;

                        if (Particles[i].VX < -0.99f)
                            Particles[i].VX = -0.99f;

                        if (Particles[i].VY > 0.99f)
                            Particles[i].VY = 0.99f;

                        if (Particles[i].VY < -0.99f)
                            Particles[i].VY = -0.99f;

                        Particles[i].PX += Particles[i].VX;
                        Particles[i].PY += Particles[i].VY;
                    }
                }
            }
        }

        public static int[] GetNeighbors(uint X, uint Y)
        {
            int[] Neighbors = new int[8];
            //UP DOWN LEFT RIGHT UP-LEFT UP-RIGHT DOWN-LEFT DOWN-RIGHT
            //0   1    2     3      4       5         6         7

            //If particle does not exist, slot evaluates to -1

            //UP
            Neighbors[0] = GetParticleAtPos(new Vector2u(X, Y - 1));

            //Down
            Neighbors[1] = GetParticleAtPos(new Vector2u(X, Y + 1));

            //Left
            Neighbors[2] = GetParticleAtPos(new Vector2u(X - 1, Y));

            //Right
            Neighbors[3] = GetParticleAtPos(new Vector2u(X+1, Y));

            //Up Left
            Neighbors[4] = GetParticleAtPos(new Vector2u(X - 1, Y-1));

            //Up Right
            Neighbors[5] = GetParticleAtPos(new Vector2u(X + 1, Y - 1));

            //Down Left
            Neighbors[6] = GetParticleAtPos(new Vector2u(X - 1, Y + 1));

            //Down Right
            Neighbors[7] = GetParticleAtPos(new Vector2u(X + 1, Y + 1));

            return Neighbors;
        }

        public static int GetParticleAtPos(Vector2u Position)
        {
            uint CurrentParticle;
            int Particle = -1;
            GridPoint Current;
            //Select the actual grid
            if (GridPoints.TryGetValue(GetGridPoint(Position.X,Position.Y), out Current))
            {
                //Check if particle exists
                if (Current.Particles.TryGetValue(Position, out CurrentParticle))
                {
                    //Neighbor exists
                    Particle = (int)CurrentParticle;
                }
            }

            return Particle;
        }

        public static void ClearGridParticles()
        {
            foreach (KeyValuePair<Vector2u, GridPoint> Entry in Solver.GridPoints)
            {
                Entry.Value.Particles.Clear();
            }
        }

        public static void AssertParticle(int i)
        {
            Vector2u CurrentPoint = GetGridPoint(Particles[i].PX, Particles[i].PY);
            Vector2u ParticlePos = new Vector2u((uint)Math.Round(Particles[i].PX), (uint)Math.Round(Particles[i].PY));
            if (!GridPoints.ContainsKey(CurrentPoint))
            {
                GridPoint NewGrid = new GridPoint();
                NewGrid.Particles.Add(ParticlePos, (uint)i);
                GridPoints.Add(CurrentPoint, NewGrid);
            }
            else
            {
                GridPoint Current;
                if (GridPoints.TryGetValue(CurrentPoint, out Current))
                {
                    if (!Current.Particles.ContainsKey(ParticlePos))
                    {
                        Current.Particles.Add(ParticlePos,(uint)i);
                    }
                }
            }
        }

        public static Vector2u GetGridPoint(float X, float Y)
        {
            uint NewX = (uint)Math.Round(X / GridSize) * GridSize;
            uint NewY = (uint)Math.Round(Y / GridSize) * GridSize;
            return new Vector2u(NewX, NewY);
        }

        public static void Remove(Vector2i Position, int Type)
        {
            int PTCLE = GetParticleAtPos((Vector2u)Position);
            if (PTCLE != -1)
            {
                GridPoint GP;
                if (GridPoints.TryGetValue(GetGridPoint(Particles[PTCLE].PX, Particles[PTCLE].PY), out GP))
                {
                    Vector2u PPos = new Vector2u((uint)Particles[PTCLE].PX, (uint)Particles[PTCLE].PY);
                    uint Particle;
                    if (GP.Particles.TryGetValue(PPos, out Particle))
                    {
                        GP.Particles.Remove(PPos);
                    }
                }
                RemoveFromParticles.Add(Particles[PTCLE]);
            }
        }

        public static void Insert(Vector2i Position, int Type)
        {
            if (GetParticleAtPos((Vector2u)Position) ==  -1)
            {
                Particles.Add(new PowderPoint()
                {
                    PX = (uint)Position.X,
                    PY = (uint)Position.Y,
                    Type = (uint)Type
                });
            }
        }
    }

    class GridPoint
    {
        public Dictionary<Vector2u,uint> Particles = new Dictionary<Vector2u,uint>();
    }
}
