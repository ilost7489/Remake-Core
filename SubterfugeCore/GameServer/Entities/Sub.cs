﻿using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;
using System;

namespace SubterfugeCore.Entities
{
    public class Sub : GameObject, ITargetable, IOwnable
    {
        private int drillerCount;
        private Vector2 initialPosition;
        private ITargetable destination;
        private GameTick launchTime;
        private float speed = 0.25f;
        private Player owner;

        public Sub(Vector2 source, ITargetable destination, GameTick launchTime, int drillerCount) : base()
        {
            this.initialPosition = source;
            this.destination = destination;
            this.launchTime = launchTime;
            this.drillerCount = drillerCount;
            this.position = source;
        }

        public Player getOwner()
        {
            return this.owner;
        }

        public GameTick getExpectedArrival()
        {
            GameTick baseTick;
            if(GameServer.state.getCurrentTick() < this.launchTime)
            {
                baseTick = this.launchTime;
            } else
            {
                baseTick = GameServer.state.getCurrentTick();
            }
            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.initialPosition);
            // Determine the number of ticks to arrive
            int ticksToArrive = (int)Math.Floor(direction.Length() / this.getSpeed());
            return baseTick.advance(ticksToArrive);
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        public override Vector2 getPosition()
        {

            // Determine number of ticks after launch:
            int elapsedTicks = GameServer.state.getCurrentTick() - this.launchTime;

            // Determine direction vector
            Vector2 direction = (this.destination.getTargetLocation(this.position, this.getSpeed()) - this.initialPosition);
            direction.Normalize();

            if(elapsedTicks > 0)
            {
                this.position = this.initialPosition + (direction * (int)(elapsedTicks * this.getSpeed()));
                return this.initialPosition + (direction * (int)(elapsedTicks * this.getSpeed()));
            }
            else
            {
                return new Vector2();
            }
        }

        public double getRotation()
        {
            // Determine direction vector
            Vector2 direction = this.destination.getTargetLocation(this.getPosition(), this.getSpeed()) - this.initialPosition;

            double extraRotation = 0;
            if(direction.X < 0)
            {
                extraRotation = Math.PI;
            }
            return Math.Atan(direction.Y / direction.X) + Math.PI/4.0f + extraRotation;
        }

        public double getSpeed()
        {
            return this.speed;
        }

        public Vector2 getInitialPosition()
        {
            return this.initialPosition;
        }

        public Vector2 getDestination()
        {
            return this.destination.getTargetLocation(this.getPosition(), this.getSpeed());
        }

        public GameTick getLaunchTick()
        {
            return this.launchTime;
        }

        public Vector2 getTargetLocation(Vector2 targetFrom, double speed)
        {
            if (targetFrom == this.getPosition())
                return targetFrom;

            if (speed == 0)
                return targetFrom;

            // Determine target's distance to travel to destination:
            Vector2 targetDestination = this.getDestination();

            // Check if the chaser can get there before it.
            Vector2 chaserDestination = targetDestination - targetFrom;

            if(Vector2.Multiply(targetDestination, (float)(1.0/this.getSpeed())).Length() > Vector2.Multiply(chaserDestination, (float)(1.0 / speed)).Length())
            {
                // Can intercept.
                // Determine interception point.

                int scalar = 1;
                bool searching = true;
                while (searching)
                {
                    Vector2 destination = this.getDestination();
                    destination.Normalize();

                    Vector2 runnerLocation = this.getPosition() + (destination * scalar);
                    Vector2 chaserDirection = runnerLocation - targetFrom;
                    chaserDirection.Normalize();
                    Vector2 chaserPosition = targetFrom + (chaserDirection * scalar);

                    if((chaserPosition - runnerLocation).Length() < 1)
                    {
                        return chaserPosition;
                    }
                    if(runnerLocation.Length() > this.getDestination().Length())
                    {
                        return targetFrom;
                    }
                    scalar = scalar + 1;
                }
                return targetFrom;

                // Interception will occur at a distance 'd' where the "ticksToArrive" at that distance are the same.

                // If both arrive at the same time,
                // tta1 = tta1
                // The ticks required to arrive at a location is given by:
                // tta = distanceToLocation / speed
                // Thus, DistanceFromRunner / RunnerSpeed = DistanceFromChaser / ChaserSpeed
                // Thus, ChaserSpeed / RunnerSpeed = DistanceFromChaser / DistanceFromRunner
                // The distance from the chaser is given as the magnitude of: targetFrom - this.Position()
                // Of course, the position of the runner changes based on the ticks to arrive.

            }
            return targetFrom;
        }

        public void setOwner(Player newOwner)
        {
            this.owner = newOwner;
        }
    }
}
