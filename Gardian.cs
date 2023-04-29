using Sovereign.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Sovereign
{
    // KI - Subclass - Armament - Gardian - Version 2.30
    public class Gardian
    {
        #region public

        public TargetingMode Mode;
        public List<int> ShipLengths;

        #endregion public

        #region private

        private Random _rnd = new Random();
        private Coordinates _lastShot;
        private Coordinates _initialHit;
        private Coordinates _lastHit;
        private List<Coordinates> _distressCoordinates;
        private List<Coordinates> _alreadyTriedCoordinates;
        private List<Coordinates> _checkBoardOcean;
        private List<Coordinates> _removedShipCoordinates;
        private List<Coordinates> _enemyShipCoordinates;
        private List<Coordinates> _hitCoordinates;
        private int _oceanSize;
        private int _hitCounter;

        #endregion private

        #region public Methods

        //Constructor
        public Gardian(int size, Battleground Sea)
        {
            //Debug.WriteLine( "Sovereign reset" )
            this._hitCounter = 0;
            this._oceanSize = size;
            ShipLengths = new List<int>();
            this._alreadyTriedCoordinates = new List<Coordinates>();
            this._enemyShipCoordinates = new List<Coordinates>();
            this._removedShipCoordinates = new List<Coordinates>();
            this._hitCoordinates = new List<Coordinates>();
            this._distressCoordinates = new List<Coordinates>();
            GenerateChessBoardOcean();
        }

        /// <summary>
        /// Controls what should happen for each specific event
        /// </summary>
        /// <param name="ShotCoordinates"> Coordinates that were returned by the AI mainframe</param>
        /// <param name="hit">bool that indicates wether this was a hit or a miss</param>
        /// <param name="deadly">bool that indicates wether this shot was deadly to the ship</param>
        public void FireControl(Coordinates ShotCoordinates, bool hit, bool deadly)
        {
            try
            {
                switch (this.Mode)
                {
                    case TargetingMode.ByChance:
                        if (hit && !deadly)
                        {
                            Debug.WriteLine("Hit by Chance");
                            this._hitCounter = this._hitCounter + 1;
                            this._lastHit = this._lastShot;
                            this._initialHit = ShotCoordinates;
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            this._hitCoordinates.Add(ShotCoordinates); ;
                            GetShipCoordinates();
                            this._enemyShipCoordinates.RemoveAll(r => r.Equals(this._lastShot));
                            MonitorTargetingMode();
                            return;
                        }
                        else if (!hit)
                        {
                            Debug.WriteLine("Miss!");
                            FillDistressCoordinates(ShotCoordinates);
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            return;
                        }
                        else if (Mode == TargetingMode.ByChance && deadly)
                        {
                            Debug.WriteLine("How bah dah?");
                            return;
                        }
                        break;
                    case TargetingMode.Aimed:
                        if (hit && deadly)
                        {
                            Debug.WriteLine("deadly!");
                            this._hitCounter = this._hitCounter + 1;
                            this._hitCoordinates.Add(ShotCoordinates);
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            ShipLengths.Remove(this._hitCounter);
                            this._enemyShipCoordinates.Clear();
                            this._removedShipCoordinates.Clear();
                            Mode = TargetingMode.ByChance;
                            this._hitCounter = 0;
                            return;
                        }
                        else if (hit && !deadly && this._enemyShipCoordinates.Count > 0)
                        {
                            Debug.WriteLine("Hit by Mode");
                            this._hitCounter = _hitCounter + 1;
                            this._lastHit = this._lastShot;
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            this._hitCoordinates.Add(ShotCoordinates);
                            this._enemyShipCoordinates.RemoveAll(r => r.Equals(this._lastShot));
                            MonitorTargetingMode();
                            return;
                        }
                        else if (!hit && this._enemyShipCoordinates.Count == 0)
                        {
                            Debug.WriteLine("Miss! Going back to By Chance Mode");
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            this._enemyShipCoordinates.Clear();
                            MonitorTargetingMode();
                            return;
                        }
                        else if (!hit && this._enemyShipCoordinates.Count > 0)
                        {
                            Debug.WriteLine("Miss! Trying other Ship Coordinates");
                            this._alreadyTriedCoordinates.Add(ShotCoordinates);
                            this._enemyShipCoordinates.RemoveAll(r => r.PointOfAim == this._lastShot.PointOfAim);
                            MonitorTargetingMode();
                            return;
                        }
                        break;
                    default:
                        return;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Random Shot Generation prefers Coordinates not included in the distressCoordinates List
        /// </summary>
        /// <returns>Coordinates of the randomly picked shot</returns>
        public Coordinates ShootByChance()
        {
            try
            {
                Coordinates shot;
                if (this._checkBoardOcean.Count() != 0)
                {
                    do
                    {
                        shot = new Coordinates(this._rnd.Next(0, this._oceanSize), this._rnd.Next(0, this._oceanSize));
                        int Index = _rnd.Next(0, this._checkBoardOcean.Count);
                        shot = this._checkBoardOcean.ElementAt(Index);
                        if (this._checkBoardOcean.Count() != 0)
                            this._checkBoardOcean.Remove(shot);
                        if (this._checkBoardOcean.Count() == 0)
                            break;
                    } while (this._alreadyTriedCoordinates.Any(a => a.Equals(shot))
                               || this._distressCoordinates.Any(a => a.Equals(shot))
                               || this._removedShipCoordinates.Any(a => a.Equals(shot)));
                }
                else if (this._distressCoordinates.Count()
                        + this._removedShipCoordinates.Count()
                        + this._alreadyTriedCoordinates.Count() < this._oceanSize * this._oceanSize)
                {
                    do
                    {
                        int rndX = this._rnd.Next(0, this._oceanSize);
                        int rndY = this._rnd.Next(0, this._oceanSize);
                        shot = new Coordinates(rndX, rndY);
                    } while (this._alreadyTriedCoordinates.Any(a => a.Equals(shot))
                               || this._distressCoordinates.Any(a => a.Equals(shot))
                               || this._removedShipCoordinates.Any(a => a.Equals(shot)));
                }
                else
                {
                    do
                    {
                        int rndX = this._rnd.Next(0, this._oceanSize);
                        int rndY = this._rnd.Next(0, this._oceanSize);
                        shot = new Coordinates(rndX, rndY);
                    } while (this._alreadyTriedCoordinates.Any(a => a.Equals(shot)));
                }
                this._lastShot = shot;
                return shot;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Shoot Method that uses the Coordinates that the AI saved as possible Coordinates of the Ship that got hit
        /// </summary>
        /// <returns>Coordinates of the shot that got chosen by the AI</returns>
        public Coordinates FireAimedShot()
        {
            try
            {
                Coordinates shot;
                if (this._enemyShipCoordinates.Count == 0)
                {
                    // Debug.WriteLine( "Ship coordinates missing" );
                }

                List<Coordinates> coordinatesNextToLastShot = new List<Coordinates>();
                coordinatesNextToLastShot = this._enemyShipCoordinates.Where(w => (w.X == this._lastHit.X + 1 && w.Y == this._lastHit.Y)
                                                                                       || (w.X == this._lastHit.X - 1 && w.Y == this._lastHit.Y)
                                                                                       || (w.Y == this._lastHit.Y + 1 && w.X == this._lastHit.X)
                                                                                       || (w.Y == this._lastHit.Y - 1 && w.X == this._lastHit.X)).ToList();
                int index;
                if (!coordinatesNextToLastShot.Any())
                {
                    Debug.WriteLine("Using inital Shot");
                    this._initialHit.PointOfAim = this._lastHit.PointOfAim;
                    this._lastHit = this._initialHit;
                    RemoveRedundantHeadings();
                    List<Coordinates> coordinatesNextToInitialHit = this._enemyShipCoordinates.Where(w => (w.X == this._initialHit.X + 1 && w.Y == this._initialHit.Y)
                                                                                          || (w.X == this._initialHit.X - 1 && w.Y == this._initialHit.Y)
                                                                                          || (w.Y == this._initialHit.Y + 1 && w.X == this._initialHit.X)
                                                                                          || (w.Y == this._initialHit.Y - 1 && w.X == this._initialHit.X)).ToList();

                    if (coordinatesNextToInitialHit.Any())
                    {
                        return FireShotOnCoordinates(coordinatesNextToInitialHit);
                    }
                    else
                    {
                        //Debug.WriteLine( "Random Shot" );
                        this._enemyShipCoordinates.Clear();
                        Mode = TargetingMode.ByChance;
                        shot = ShootByChance();
                        this._lastShot = shot;
                        return shot;
                    }
                }
                return FireShotOnCoordinates(coordinatesNextToLastShot);
            catch (Exception)
            {
                throw;
            }
        }

        #endregion public Methods

        #region private Methods

        //Generates a Chessfield similiar List of Coordinates of the Battfield to decrease the amount of random shots
        private void GenerateChessBoardOcean()
        {
            try
            {
                this._checkBoardOcean = new List<Coordinates>();
                for (int y = 0; y < this._oceanSize; y++)
                {
                    var result = y % 2;
                    if (result == 0)
                    {
                        for (int x = 0; x < this._oceanSize; x = x + 2)
                        {
                            Coordinates BlackChessFieldCoordinates = new Coordinates(x, y);
                            this._checkBoardOcean.Add(BlackChessFieldCoordinates);
                        }
                    }
                    else
                    {
                        for (int x = 1; x < this._oceanSize; x = x + 2)
                        {
                            Coordinates BlackOddChessFieldCoordinates = new Coordinates(x, y);
                            this._checkBoardOcean.Add(BlackOddChessFieldCoordinates);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        /// <summary>
        /// Determins the next Shot base on Random determination
        /// </summary>
        /// <param name="shot">Coordinates of possible shots</param>
        /// <returns> the Coordinate of the Shot</returns
        private Coordinates FireShotOnCoordinates(List<Coordinates> coordinatesNextToLastShot)
        {
            int index = this._rnd.Next(0, coordinatesNextToLastShot.Count());
            Coordinates shot = coordinatesNextToLastShot.ElementAt(index);
            this._lastShot = shot;
            return shot;
        }

        /// <summary>
        /// Fills a List of Coordinates that are surrounding the shot and will be Used when viable ShipCoordinates run out
        /// </summary>
        /// <param name="shot">Coordinates of the last shot</param>
        private void FillDistressCoordinates(Coordinates shot)
        {
            foreach (var hitCoordinate in this._hitCoordinates)
                this._distressCoordinates.Remove(hitCoordinate);

            if (!this._distressCoordinates.Any(a => a.Equals(a.X == shot.X && a.Y == shot.Y + 1))
                    && ((shot.Y + 1) < this._oceanSize)
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(a.X == shot.X && a.Y == shot.Y + 1)))
                this._distressCoordinates.Add(new Coordinates(shot.X, shot.Y + 1));

            if ((!this._distressCoordinates.Any(a => a.Equals(a.X == shot.X - 1 && a.Y == shot.Y))
                    && (shot.X - 1) >= 0)
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(a.X == shot.X - 1 && a.Y == shot.Y)))
                this._distressCoordinates.Add(new Coordinates(shot.X - 1, shot.Y));

            if ((!this._alreadyTriedCoordinates.Any(a => a.Equals(a.X == shot.X + 1 && a.Y == shot.Y))
                    && (shot.X + 1) < this._oceanSize)
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(a.X == shot.X - 1 && a.Y == shot.Y)))
                this._distressCoordinates.Add(new Coordinates(shot.X + 1, shot.Y));

            if ((!this._distressCoordinates.Any(a => a.Equals(a.X == shot.X && a.Y == shot.Y - 1))
                    && (shot.Y - 1) >= 0)
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(a.X == shot.X && a.Y == shot.Y - 1)))
                this._distressCoordinates.Add(new Coordinates(shot.X, shot.Y - 1));
        }

        // Monitors if the Targeting Mode has to be forced to change
        private void MonitorTargetingMode()
        {
            if (this._enemyShipCoordinates.Count == 0 && Mode == TargetingMode.Aimed)
            {
                //if ( this._removedShipCoordinates.Count != 0 )
                //{
                //    Debug.WriteLine( "Monitor overrides '_shipCoordinates' " );
                //    this._shipCoordinates = this._removedShipCoordinates;
                //}
                //else
                //{
                //   Debug.WriteLine( "Monitor forces By Chance Mode" );
                this._enemyShipCoordinates.Clear();
                Mode = TargetingMode.ByChance;
                //}
            }
            else
            {
                Mode = TargetingMode.Aimed;
            }
        }

        // Saves all possible Coordinates that could be occupied by the Ship, that got hit, in a List
        private void GetShipCoordinates()
        {
            Coordinates shotUpwards;
            Coordinates shotLeftwards;
            Coordinates shotRightwards;
            Coordinates shotDownwards;
            int maxShipLength;

            if (Mode == TargetingMode.Aimed)
                RemoveRedundantHeadings();

            maxShipLength = ShipLengths.Max();

            for (int i = 1; i < maxShipLength; i++)
            {
                shotUpwards = new Coordinates(this._lastHit.X, this._lastHit.Y - i);
                shotUpwards.PointOfAim = Heading.Up;
                if (this._lastHit.Y - i >= 0
                   && this._lastHit.PointOfAim != Heading.Left
                   && this._lastHit.PointOfAim != Heading.Right
                   && !this._alreadyTriedCoordinates.Any(a => a.Equals(shotUpwards))
                   && !this._hitCoordinates.Any(a => a.Equals(shotUpwards)))
                {
                    this._enemyShipCoordinates.Add(shotUpwards);
                }
            }

            for (int i = 1; i < maxShipLength; i++)
            {
                shotDownwards = new Coordinates(this._lastHit.X, this._lastHit.Y + i);
                shotDownwards.PointOfAim = Heading.Down;
                if (this._lastHit.Y + i < this._oceanSize
                    && this._lastHit.PointOfAim != Heading.Left
                    && this._lastHit.PointOfAim != Heading.Right
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(shotDownwards))
                    && !this._hitCoordinates.Any(a => a.Equals(shotDownwards)))
                {
                    this._enemyShipCoordinates.Add(shotDownwards);
                }
            }

            for (int i = 1; i < maxShipLength; i++)
            {
                shotLeftwards = new Coordinates(this._lastHit.X - i, this._lastHit.Y);
                shotLeftwards.PointOfAim = Heading.Left;
                if (this._lastHit.X - i >= 0
                    && this._lastHit.PointOfAim != Heading.Up
                    && this._lastHit.PointOfAim != Heading.Down
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(shotLeftwards))
                    && !this._hitCoordinates.Any(a => a.Equals(shotLeftwards)))
                {
                    this._enemyShipCoordinates.Add(shotLeftwards);
                }
            }

            for (int i = 1; i < maxShipLength; i++)
            {
                shotRightwards = new Coordinates(this._lastHit.X + i, this._lastHit.Y);
                shotRightwards.PointOfAim = Heading.Right;
                if (this._lastHit.X + i < this._oceanSize
                    && this._lastHit.PointOfAim != Heading.Up
                    && this._lastHit.PointOfAim != Heading.Down
                    && !this._alreadyTriedCoordinates.Any(a => a.Equals(shotRightwards))
                    && !this._hitCoordinates.Any(a => a.Equals(shotRightwards)))
                {
                    this._enemyShipCoordinates.Add(shotRightwards);
                }
            }
        }

        // Removes the Coordinates which Heading differs vertically or horizontally from the Direction where a TargetingMode-driven Hit landed
        private void RemoveRedundantHeadings()
        {
            //Vertical Shot hit
            if (this._lastHit.PointOfAim == Heading.Up || this._lastHit.PointOfAim == Heading.Down)
            {
                this._removedShipCoordinates.AddRange(this._enemyShipCoordinates.Where(w => w.PointOfAim == Heading.Left || w.PointOfAim == Heading.Right).ToList());
                this._enemyShipCoordinates.RemoveAll(r => r.PointOfAim == Heading.Left || r.PointOfAim == Heading.Right);
            }
            //Horizontal Shot hit
            if (this._lastHit.PointOfAim == Heading.Left || this._lastHit.PointOfAim == Heading.Right)
            {
                this._removedShipCoordinates.AddRange(this._enemyShipCoordinates.Where(w => w.PointOfAim == Heading.Up || w.PointOfAim == Heading.Down).ToList());
                this._enemyShipCoordinates.RemoveAll(r => r.PointOfAim == Heading.Up || r.PointOfAim == Heading.Down);
            }
        }

        #endregion private Methods
    }
}