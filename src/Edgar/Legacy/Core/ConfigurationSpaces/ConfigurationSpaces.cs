﻿using System;
using System.Collections.Generic;
using System.Linq;
using Edgar.Geometry;
using Edgar.GraphBasedGenerator.Common.ConfigurationSpaces;
using Edgar.Legacy.Core.Configurations.Interfaces;
using Edgar.Legacy.Core.MapDescriptions;
using Edgar.Legacy.GeneralAlgorithms.Algorithms.Common;
using Edgar.Legacy.GeneralAlgorithms.DataStructures.Common;
using Edgar.Legacy.GeneralAlgorithms.DataStructures.Polygons;

namespace Edgar.Legacy.Core.ConfigurationSpaces
{
    /// <summary>
    /// Implementation of configuration spaces. This class only retrieves data generated by ConfigurationSpacesGenerator.
    /// </summary>
    /// <typeparam name="TConfiguration"></typeparam>
    public class ConfigurationSpaces<TConfiguration> :
        AbstractConfigurationSpaces<int, IntAlias<PolygonGrid2D>, TConfiguration>, IConfigurationSpaces<TConfiguration>
        where TConfiguration : IConfiguration<IntAlias<PolygonGrid2D>, int>
    {
        private readonly Func<TConfiguration, TConfiguration, int> configurationSpaceSelector;
        protected List<List<WeightedShape>> ShapesForNodes;
        protected ConfigurationSpace[][][] ConfigurationSpaces_;

        protected TwoWayDictionary<RoomTemplateInstance, IntAlias<PolygonGrid2D>> IntAliasMapping =
            new TwoWayDictionary<RoomTemplateInstance, IntAlias<PolygonGrid2D>>();

        public ConfigurationSpaces(
            ILineIntersection<OrthogonalLineGrid2D> lineIntersection, int roomTemplateInstancesCount, int nodesCount,
            Func<TConfiguration, TConfiguration, int> configurationSpaceSelector) : base(lineIntersection)
        {
            this.configurationSpaceSelector = configurationSpaceSelector;
            // Init configuration spaces array
            ConfigurationSpaces_ = new ConfigurationSpace[roomTemplateInstancesCount][][];
            for (var i = 0; i < roomTemplateInstancesCount; i++)
            {
                ConfigurationSpaces_[i] = new ConfigurationSpace[roomTemplateInstancesCount][];
            }

            // Init shapes for node lists
            ShapesForNodes = new List<List<WeightedShape>>(nodesCount);
            for (var i = 0; i < nodesCount; i++)
            {
                ShapesForNodes.Add(new List<WeightedShape>());
            }
        }

        /// <inheritdoc />
        protected override IList<Tuple<TConfiguration, ConfigurationSpace>> GetConfigurationSpaces(
            TConfiguration mainConfiguration, IList<TConfiguration> configurations)
        {
            var spaces = new List<Tuple<TConfiguration, ConfigurationSpace>>();
            var chosenSpaces = ConfigurationSpaces_[mainConfiguration.ShapeContainer.Alias];

            foreach (var configuration in configurations)
            {
                spaces.Add(Tuple.Create(configuration,
                    chosenSpaces[configuration.ShapeContainer.Alias][
                        configurationSpaceSelector(mainConfiguration, configuration)]));
            }

            return spaces;
        }

        /// <inheritdoc />
        public override ConfigurationSpace GetConfigurationSpace(TConfiguration mainConfiguration,
            TConfiguration configuration)
        {
            return ConfigurationSpaces_[mainConfiguration.ShapeContainer.Alias][configuration.ShapeContainer.Alias][
                configurationSpaceSelector(mainConfiguration, configuration)];
        }

        /// <inheritdoc />
        public override ConfigurationSpace GetConfigurationSpace(IntAlias<PolygonGrid2D> movingPolygon,
            IntAlias<PolygonGrid2D> fixedPolygon)
        {
            throw new InvalidOperationException();
            return ConfigurationSpaces_[movingPolygon.Alias][fixedPolygon.Alias][0]; // TODO: is this ok?
        }

        /// <inheritdoc />
        /// <summary>
        /// Get random shape for a given node based on probabilities of shapes.
        /// </summary>
        public override IntAlias<PolygonGrid2D> GetRandomShape(int node)
        {
            return ShapesForNodes[node].GetWeightedRandom(x => x.Weight, Random).Shape;
        }

        /// <inheritdoc />
        public override bool CanPerturbShape(int node)
        {
            // We need at least 2 shapes to choose from for it to be perturbed
            return ShapesForNodes[node].Count >= 2;
        }

        /// <inheritdoc />
        public override IReadOnlyCollection<IntAlias<PolygonGrid2D>> GetShapesForNode(int node)
        {
            return ShapesForNodes[node].Select(x => x.Shape).ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public override IEnumerable<IntAlias<PolygonGrid2D>> GetAllShapes()
        {
            var usedShapes = new HashSet<int>();

            foreach (var shapes in ShapesForNodes)
            {
                if (shapes == null)
                    continue;

                foreach (var shape in shapes)
                {
                    if (!usedShapes.Contains(shape.Shape.Alias))
                    {
                        yield return shape.Shape;
                        usedShapes.Add(shape.Shape.Alias);
                    }
                }
            }
        }

        public TwoWayDictionary<RoomTemplateInstance, IntAlias<PolygonGrid2D>> GetIntAliasMapping()
        {
            return IntAliasMapping;
        }

        public void AddConfigurationSpace(RoomTemplateInstance roomTemplateInstance1,
            RoomTemplateInstance roomTemplateInstance2, ConfigurationSpace[] configurationSpace)
        {
            var alias1 = GetRoomTemplateInstanceAlias(roomTemplateInstance1);
            var alias2 = GetRoomTemplateInstanceAlias(roomTemplateInstance2);

            ConfigurationSpaces_[alias1.Alias][alias2.Alias] = configurationSpace;
        }

        public void AddShapeForNode(int node, RoomTemplateInstance roomTemplateInstance, double probability)
        {
            var alias = GetRoomTemplateInstanceAlias(roomTemplateInstance);

            ShapesForNodes[node].Add(new WeightedShape(alias, probability));
        }

        private IntAlias<PolygonGrid2D> GetRoomTemplateInstanceAlias(RoomTemplateInstance roomTemplateInstance)
        {
            if (IntAliasMapping.TryGetValue(roomTemplateInstance, out var alias))
            {
                return alias;
            }

            var newAlias = new IntAlias<PolygonGrid2D>(IntAliasMapping.Count, roomTemplateInstance.RoomShape);

            IntAliasMapping.Add(roomTemplateInstance, newAlias);

            return newAlias;
        }
    }
}