﻿using System;
using System.Collections.Generic;
using Edgar.GraphBasedGenerator.Common.RoomTemplates;

namespace Edgar.GraphBasedGenerator.Grid2D
{
    /// <summary>
    /// Describes the properties of a single room in a level.
    /// </summary>
    public class RoomDescriptionGrid2D : IRoomDescription
    {
        /// <summary>
        /// Whether the room is a corridor or not.
        /// </summary>
        public bool IsCorridor { get; }

        /// <summary>
        /// Room templates available for the room.
        /// </summary>
        public List<RoomTemplateGrid2D> RoomTemplates { get; }

        /// <param name="isCorridor">See the <see cref="IsCorridor"/> property.</param>
        /// <param name="roomTemplates">See the <see cref="RoomTemplates"/> property.</param>
        public RoomDescriptionGrid2D(bool isCorridor, List<RoomTemplateGrid2D> roomTemplates)
        {
            ArgumentNullException.ThrowIfNull(roomTemplates);

            if (roomTemplates.Count == 0)
                throw new ArgumentException("Value cannot be an empty collection.", nameof(roomTemplates));

            IsCorridor = isCorridor;
            RoomTemplates = roomTemplates;
        }
    }
}