/*
 * Copyright (c) 2023 Proton AG
 *
 * This file is part of ProtonVPN.
 *
 * ProtonVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * ProtonVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with ProtonVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using ProtonVPN.Logging.Contracts;
using ProtonVPN.Logging.Contracts.Events.AppLogs;
using ProtonVPN.EntityMapping.Contracts;

namespace ProtonVPN.EntityMapping
{
    public class EntityMapper : IEntityMapper
    {
        private Lazy<IDictionary<Tuple<Type, Type>, IMapper>> _mappers;
        private readonly ILogger _logger;

        public EntityMapper(ILogger logger, Lazy<IEnumerable<IMapper>> mappers)
        {
            _logger = logger;
            _mappers = new Lazy<IDictionary<Tuple<Type, Type>, IMapper>>(() => CreateMappers(mappers));
        }

        private IDictionary<Tuple<Type, Type>, IMapper> CreateMappers(Lazy<IEnumerable<IMapper>> lazyMappers)
        {
            Dictionary<Tuple<Type, Type>, IMapper> mapperDictionary = new();
            IEnumerable<IMapper> mappers = lazyMappers.Value;
            foreach (IMapper mapper in mappers)
            {
                Type genericInterface = mapper.GetType().GetInterfaces()
                    .Where(t => t.IsGenericType && t.GenericTypeArguments.Length == 2)
                    .Single();
                mapperDictionary.Add(new Tuple<Type, Type>(
                    genericInterface.GenericTypeArguments[0],
                    genericInterface.GenericTypeArguments[1]), mapper);
            }

            return mapperDictionary;
        }

        public TOutput Map<TInput, TOutput>(TInput inputEntity)
        {
            if (_mappers.Value.TryGetValue(new Tuple<Type, Type>(typeof(TInput), typeof(TOutput)), out IMapper genericMapper))
            {
                return ((IMapper<TInput, TOutput>)genericMapper).Map(inputEntity);
            }
            if (_mappers.Value.TryGetValue(new Tuple<Type, Type>(typeof(TOutput), typeof(TInput)), out IMapper inverseGenericMapper))
            {
                return ((IMapper<TOutput, TInput>)inverseGenericMapper).Map(inputEntity);
            }

            string errorMessage = $"Cannot find Entity Mapper between types '{typeof(TInput).FullName}' and '{typeof(TOutput).FullName}'.";
            _logger.Fatal<AppLog>(errorMessage);
            throw new NotImplementedException(errorMessage);
        }

        public TOutput? MapNullableStruct<TInput, TOutput>(TInput? inputEntity)
            where TInput : struct
            where TOutput : struct
        {
            return inputEntity == null ? null : Map<TInput, TOutput>(inputEntity.Value);
        }

        public List<TOutput> Map<TInput, TOutput>(IEnumerable<TInput> inputEntities)
        {
            List<TOutput> mappedEntities = [];
            if (inputEntities is null)
            {
                return mappedEntities;
            }
            foreach (TInput inputEntity in inputEntities)
            {
                TOutput outputEntity = Map<TInput, TOutput>(inputEntity);
                if (outputEntity is not null)
                {
                    mappedEntities.Add(outputEntity);
                }
            }
            return mappedEntities;
        }
    }
}