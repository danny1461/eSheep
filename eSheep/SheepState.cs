using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace eSheep
{
    // Base Class
    abstract class SheepState
    {
        public const int FALLING_SPEED = 12;

        public Sheep parent;
        public int Direction = -1;
        public bool OnGround = true;
        protected byte frameDelay = 2;
        public virtual byte FrameDelay { get { return 2; } }
        public bool DrawSprite { get { if (forceRender) { forceRender = false; return true; } return frameDelay == FrameDelay; } }
        public virtual bool StandingUp
        {
            get
            {
                return true;
            }
        }

        public abstract List<Point> Frames { get; }
        protected int currentFrame = 0;
        public Point CurrentAnimation
        {
            get
            {
                return Frames[currentFrame];
            }
        }

        public void ForceRender()
        {
            forceRender = true;
        }
        private bool forceRender = false;
        public void StartAnimation()
        {
            frameDelay = FrameDelay;
        }

        public virtual void UpdateGravity()
        {
            if (OnGround)
            {
                OnGround = false;
                foreach (Rectangle rect in parent.manager.windows)
                {
                    if (parent.BoundingBox.Right > rect.Left && parent.BoundingBox.Left < rect.Right)
                    {
                        if (parent.BoundingBox.IntersectsWith(rect) && (parent.BoundingBox.Top - rect.Top) < 0)
                        {
                            OnGround = true;
                            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, rect.Top - Sheep.SheepSize);
                            break;
                        }
                        else if (parent.BoundingBox.Bottom == rect.Top)
                        {
                            OnGround = true;
                            break;
                        }
                    }
                }
                if (!OnGround)
                {
                    foreach (Sheep sheep in parent.manager.sheep)
                    {
                        if (sheep.ID == parent.ID)
                            continue;
                        if (parent.BoundingBox.Bottom == sheep.BoundingBox.Top && parent.BoundingBox.Right > sheep.BoundingBox.Left && parent.BoundingBox.Left < sheep.BoundingBox.Right)
                        {
                            OnGround = true;
                            break;
                        }
                    }
                }
                if (!OnGround)
                {
                    // Start Falling
                    parent.ForceChangeState(new FallingStateUpright(parent, Direction));
                }
            }
            else
            {
                parent.BoundingBox.Offset(0, FALLING_SPEED);
                foreach (Rectangle rect in parent.manager.windows)
                {
                    if (parent.BoundingBox.IntersectsWith(rect) && (parent.BoundingBox.Top - rect.Top) < 0)
                    {
                        OnGround = true;
                        OnFallOnWindow(rect);
                        break;
                    }
                }
                if (!OnGround)
                {
                    foreach (Sheep sheep in parent.manager.sheep)
                    {
                        if (sheep.ID == parent.ID)
                            continue;
                        if (parent.BoundingBox.IntersectsWith(sheep.BoundingBox))
                        {
                            OnGround = true;
                            OnFallOnSheep(sheep);
                            break;
                        }
                    }
                }
            }
        }
        public virtual void OnFallOnWindow(Rectangle window)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, window.Top - Sheep.SheepSize);
            parent.ChangeState(new AfterFallingState(parent, Direction));
        }
        public virtual void OnFallOnSheep(Sheep sheep)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, sheep.BoundingBox.Top - Sheep.SheepSize);
            if (sheep.CurrentState.StandingUp)
                sheep.ForceChangeState(new FlattenedState(sheep, sheep.CurrentState.Direction));
            parent.ChangeState(new AfterFallingState(parent, Direction));
        }

        public virtual void Update()
        {
            if (parent.NextState != null)
                return;
            frameDelay--;
            if (frameDelay == 0)
            {
                currentFrame++;
                if (currentFrame == Frames.Count)
                {
                    currentFrame = 0;
                    AnimationComplete();
                }
                frameDelay = FrameDelay;
            }
        }

        public virtual void AnimationComplete()
        {

        }
    }

    class IdleState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(3, 0)
                };
            }
        }

        public IdleState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();
            switch (Form1.rand.Next(0, 200))
            {
                case 0:             // Walk Left
                case 1:
                    if (Direction == -1)
                        parent.ChangeState(new WalkState(parent, Direction));
                    else
                        parent.ChangeState(new TurnAndMove(parent, Direction, true));
                    break;
                case 2:             // Walk Right
                case 3:
                    if (Direction == 1)
                        parent.ChangeState(new WalkState(parent, Direction));
                    else
                        parent.ChangeState(new TurnAndMove(parent, Direction, true));
                    break;
                case 4:             // Run Left
                case 5:
                    if (Direction == -1)
                        parent.ChangeState(new RunState(parent, Direction));
                    else
                        parent.ChangeState(new TurnAndMove(parent, Direction, false));
                    break;
                case 6:             // Run Right
                case 7:
                    if (Direction == 1)
                        parent.ChangeState(new RunState(parent, Direction));
                    else
                        parent.ChangeState(new TurnAndMove(parent, Direction, false));
                    break;
                case 8:
                case 9:
                    parent.ChangeState(new SleepState(parent, Direction));
                    break;
                case 10:             // Climb
                case 11:
                    bool shouldClimb = false;
                    foreach (Rectangle rect in parent.manager.windows)
                    {
                        if (rect.Y >= parent.BoundingBox.Bottom)
                            continue;
                        if (rect.X <= parent.BoundingBox.X && rect.Right >= parent.BoundingBox.Right)
                            shouldClimb = true;
                    }
                    if (shouldClimb)
                        parent.ChangeState(new ClimbingState(parent, Direction));
                    break;
            }

            base.Update();
        }
    }

    class SleepState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 0),
                    new Point(0, 0),
                    new Point(1, 0)
                };
            }
        }

        public override byte FrameDelay
        {
            get
            {
                return 6;
            }
        }

        public SleepState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();
            switch (Form1.rand.Next(0, 150))
            {
                case 0:
                    parent.ChangeState(new IdleState(parent, Direction));
                    break;
            }

            base.Update();
        }
    }

    class WalkState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                    {
                        new Point(3, 0),
                        new Point(2, 0)
                    };
            }
        }

        public WalkState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();
            parent.BoundingBox.Offset(3 * Direction, 0);
            if (parent.BoundingBox.Left <= 0)
            {
                parent.BoundingBox.Location = new Point(0, parent.BoundingBox.Top);
                parent.ChangeState(new IdleState(parent, Direction));
            }
            else if (parent.BoundingBox.Right >= SystemInformation.VirtualScreen.Right)
            {
                parent.BoundingBox.Location = new Point(SystemInformation.VirtualScreen.Right - Sheep.SheepSize, parent.BoundingBox.Top);
                parent.ChangeState(new IdleState(parent, Direction));
            }
            else
            {
                switch (Form1.rand.Next(0, 100))
                {
                    case 0:         // Stop Walking
                        parent.ChangeState(new IdleState(parent, Direction));
                        break;
                }
            }

            base.Update();
        }
    }
    class RunState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                    {
                        new Point(4, 0),
                        new Point(5, 0)
                    };
            }
        }

        public RunState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();
            parent.BoundingBox.Offset(5 * Direction, 0);
            if (parent.BoundingBox.Left <= 0)
            {
                parent.BoundingBox.Location = new Point(0, parent.BoundingBox.Top);
                parent.ChangeState(new IdleState(parent, Direction));
            }
            else if (parent.BoundingBox.Right >= SystemInformation.VirtualScreen.Right)
            {
                parent.BoundingBox.Location = new Point(SystemInformation.VirtualScreen.Right - Sheep.SheepSize, parent.BoundingBox.Top);
                parent.ChangeState(new IdleState(parent, Direction));
            }
            else
            {
                switch (Form1.rand.Next(0, 100))
                {
                    case 0:         // Stop Running
                        parent.ChangeState(new IdleState(parent, Direction));
                        break;
                }
            }

            base.Update();
        }
    }
    class TurnAndMove : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                    {
                        new Point(9, 0),
                        new Point(10, 0),
                        new Point(11, 0)
                    };
            }
        }
        private bool walk;

        public TurnAndMove(Sheep parent, int dir, bool walk)
        {
            this.parent = parent;
            Direction = dir;
            this.walk = walk;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            if (walk)
                parent.ChangeState(new WalkState(parent, Direction == -1 ? 1 : -1));
            else
                parent.ChangeState(new RunState(parent, Direction == -1 ? 1 : -1));
        }
    }

    class FallingStateUpright : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(2, 3),
                    new Point(3, 3)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }
        private byte waitOne = 5;

        public FallingStateUpright(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            OnGround = false;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            waitOne--;
            if (waitOne == 0)
            {
                if (Form1.rand.Next(0, 2) == 0)
                    parent.ChangeState(new FallingFlipUpsideDown1(parent, Direction));
                else
                    parent.ChangeState(new FallingFlipUpsideDown2(parent, Direction));
            }
        }

        public override void OnFallOnWindow(Rectangle window)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, window.Top - Sheep.SheepSize);
            parent.ChangeState(new IdleState(parent, Direction));
        }
        public override void OnFallOnSheep(Sheep sheep)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, sheep.BoundingBox.Top - Sheep.SheepSize);
            if (sheep.CurrentState.StandingUp)
                sheep.ForceChangeState(new FlattenedState(sheep, sheep.CurrentState.Direction));
            parent.ChangeState(new IdleState(parent, Direction));
        }
    }
    class FallingFlipUpsideDown1 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(13, 5),
                    new Point(13, 5),
                    new Point(3, 6),
                    new Point(4, 6)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }

        public FallingFlipUpsideDown1(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            OnGround = false;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new FallingStateUpsideDown(parent, Direction));
        }
    }
    class FallingFlipUpsideDown2 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 6),
                    new Point(0, 6),
                    new Point(3, 6),
                    new Point(4, 6)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }

        public FallingFlipUpsideDown2(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            OnGround = false;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new FallingStateUpsideDown(parent, Direction));
        }
    }
    class FallingStateUpsideDown : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(4, 6),
                    new Point(3, 6)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }

        public FallingStateUpsideDown(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            OnGround = false;
        }

        public override void Update()
        {
            UpdateGravity();

            // Recycle sheep
            if (parent.BoundingBox.Top > Screen.PrimaryScreen.WorkingArea.Bottom * 1.5)
            {
                parent.BoundingBox.X = Form1.rand.Next(0, SystemInformation.VirtualScreen.Width - Sheep.SheepSize);
                parent.BoundingBox.Y = Form1.rand.Next(-600, -100);
            }

            base.Update();
        }
    }
    class AfterFallingState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(4, 6)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }
        private int waitPeriod = 0;

        public AfterFallingState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            waitPeriod = Form1.rand.Next(10, 40);
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            waitPeriod--;
            if (waitPeriod == 0)
            {
                if (Form1.rand.Next(0, 2) == 0)
                    parent.ChangeState(new AfterFallingWaitOne1(parent, Direction));
                else
                    parent.ChangeState(new AfterFallingWaitOne2(parent, Direction));
            }
        }
    }
    class AfterFallingWaitOne1 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(0, 6),
                    new Point(0, 6),
                    new Point(0, 6),
                    new Point(0, 6),
                    new Point(8, 3),
                    new Point(13, 6),
                    new Point(13, 6),
                    new Point(3, 0)
                };
            }
        }

        public AfterFallingWaitOne1(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new IdleState(parent, Direction));
        }
    }
    class AfterFallingWaitOne2 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(2, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(1, 6),
                    new Point(13, 5),
                    new Point(13, 5),
                    new Point(13, 5),
                    new Point(13, 5),
                    new Point(8, 3),
                    new Point(13, 6),
                    new Point(13, 6),
                    new Point(3, 0)
                };
            }
        }

        public AfterFallingWaitOne2(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new IdleState(parent, Direction));
        }
    }
    class FlattenedState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(7, 1)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }
        private int waitPeriod = 0;

        public FlattenedState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
            waitPeriod = Form1.rand.Next(10, 40);
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            waitPeriod--;
            if (waitPeriod == 0)
                parent.ChangeState(new AfterFlattenedStateWaitOne(parent, Direction));
        }
    }
    class AfterFlattenedStateWaitOne : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(8, 3),
                    new Point(13, 6),
                    new Point(13, 6),
                    new Point(14, 6),
                    new Point(14, 6),
                    new Point(3, 0),
                };
            }
        }

        public AfterFlattenedStateWaitOne(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new IdleState(parent, Direction));
        }
    }

    class ClimbingState : SheepState
    {
        private int climbStart = 0;

        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(3, 8),
                    new Point(4, 8)
                };
            }
        }

        public override byte FrameDelay
        {
            get
            {
                return 4;
            }
        }

        public ClimbingState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;

            climbStart = parent.BoundingBox.Top;
        }

        public override void Update()
        {
            // Run base first to sync the movement with the animation
            base.Update();

            if (DrawSprite)
            {
                parent.BoundingBox.Offset(0, -5);
            }

            if (climbStart - parent.BoundingBox.Top > 140)
            {
                if (parent.BoundingBox.Y < -200)
                {
                    parent.ChangeState(new ClimbingSlippingState(parent, Direction));
                    return;
                }
                foreach (Rectangle rect in parent.manager.windows)
                {
                    if (parent.BoundingBox.Right > rect.Left && parent.BoundingBox.Left < rect.Right)
                    {
                        if (parent.BoundingBox.IntersectsWith(rect) && (parent.BoundingBox.Top - rect.Top) <= -30)
                        {
                            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, rect.Top - Sheep.SheepSize);
                            parent.ChangeState(new IdleState(parent, Form1.rand.Next(0, 2) == 0 ? -1 : 1));
                            break;
                        }
                    }
                }
                switch (Form1.rand.Next(0, 200))
                {
                    case 0:                 // Slip and fall
                        parent.ChangeState(new ClimbingSlippingState(parent, Direction));
                        break;
                }
            }
        }
    }
    class ClimbingSlippingState : SheepState
    {
        int waitPeriod = 4;

        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(5, 8)
                };
            }
        }

        public override byte FrameDelay
        {
            get
            {
                return 4;
            }
        }

        public ClimbingSlippingState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }

        public override void AnimationComplete()
        {
            waitPeriod--;
            parent.BoundingBox.Offset(0, 2);
            if (waitPeriod == 0)
                parent.ChangeState(new ClimbingFallingState(parent, Direction));
        }
    }
    class ClimbingFallingState : SheepState
    {
        private int fallStart = 0;

        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(5, 8)
                };
            }
        }

        public ClimbingFallingState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;

            fallStart = this.parent.BoundingBox.Top;
            OnGround = false;
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void OnFallOnWindow(Rectangle window)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, window.Top - Sheep.SheepSize);
            if ((parent.BoundingBox.Top - fallStart) > 150)
                parent.ChangeState(new AfterClimbingFallingHurtState(parent, Direction));
            else
                parent.ChangeState(new AfterClimbingFallingUnhurtState(parent, Direction));
        }

        public override void OnFallOnSheep(Sheep sheep)
        {
            parent.BoundingBox.Location = new Point(parent.BoundingBox.Left, sheep.BoundingBox.Top - Sheep.SheepSize);
            if (sheep.CurrentState.StandingUp)
                sheep.ForceChangeState(new FlattenedState(sheep, sheep.CurrentState.Direction));

            if ((parent.BoundingBox.Top - fallStart) > 150)
                parent.ChangeState(new AfterClimbingFallingHurtState(parent, Direction));
            else
                parent.ChangeState(new AfterClimbingFallingUnhurtState(parent, Direction));
        }
    }
    class AfterClimbingFallingHurtState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(0, 3)
                };
            }
        }

        public override byte FrameDelay
        {
            get
            {
                return (byte)Form1.rand.Next(20, 40);
            }
        }

        public AfterClimbingFallingHurtState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;

            ForceRender();
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            if (Form1.rand.Next(0, 2) == 0)
                parent.ChangeState(new AfterClimbingFallingStandUpState1(parent));
            else
                parent.ChangeState(new AfterClimbingFallingStandUpState2(parent));
        }
    }
    class AfterClimbingFallingUnhurtState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(1, 3)
                };
            }
        }

        public override byte FrameDelay
        {
            get
            {
                return (byte)Form1.rand.Next(15, 30);
            }
        }

        public AfterClimbingFallingUnhurtState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;

            ForceRender();
        }

        public override void Update()
        {
            UpdateGravity();

            base.Update();
        }

        public override void AnimationComplete()
        {
            if (Form1.rand.Next(0, 2) == 0)
                parent.ChangeState(new AfterClimbingFallingStandUpState1(parent));
            else
                parent.ChangeState(new AfterClimbingFallingStandUpState2(parent));
        }
    }
    class AfterClimbingFallingStandUpState1 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(1, 3),
                    new Point(13, 0),
                    new Point(14, 0)
                };
            }
        }

        public AfterClimbingFallingStandUpState1(Sheep parent)
        {
            this.parent = parent;
            Direction = -1;
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new IdleState(parent, Direction == 1 ? -1 : 1));
        }
    }
    class AfterClimbingFallingStandUpState2 : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(1, 3),
                    new Point(13, 0),
                    new Point(14, 0)
                };
            }
        }

        public AfterClimbingFallingStandUpState2(Sheep parent)
        {
            this.parent = parent;
            Direction = 1;
        }

        public override void AnimationComplete()
        {
            parent.ChangeState(new IdleState(parent, Direction == 1 ? -1 : 1));
        }
    }

    class DraggingState : SheepState
    {
        public override List<Point> Frames
        {
            get
            {
                return new List<Point>
                {
                    new Point(2, 3),
                    new Point(3, 3)
                };
            }
        }
        public override bool StandingUp
        {
            get
            {
                return false;
            }
        }

        public DraggingState(Sheep parent, int dir)
        {
            this.parent = parent;
            Direction = dir;
        }
    }
}
