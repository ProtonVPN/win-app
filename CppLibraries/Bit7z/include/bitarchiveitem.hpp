/*
 * bit7z - A C++ static library to interface with the 7-zip DLLs.
 * Copyright (c) 2014-2019  Riccardo Ostani - All Rights Reserved.
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * Bit7z is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with bit7z; if not, see https://www.gnu.org/licenses/.
 */

#ifndef BITARCHIVEITEMREADER_HPP
#define BITARCHIVEITEMREADER_HPP

#include <map>

#include "../include/bitpropvariant.hpp"

namespace bit7z {
    using std::wstring;
    using std::map;

    /**
     * @brief The BitArchiveItem class represents an item contained in an archive and contains all its properties.
     */
    class BitArchiveItem {
        public:
            /**
             * @brief BitArchiveItem destructor.
             */
            virtual ~BitArchiveItem();

            /**
             * @return the index of the item in the archive.
             */
            uint32_t index() const;

            /**
             * @return true if and only if the item is a directory (i.e. it has the property BitProperty::IsDir).
             */
            bool isDir() const;

            /**
             * @return the name of the item, if available or inferable from the path, or an empty string otherwise.
             */
            wstring name() const;

            /**
             * @return the extension of the item, if available or inferable from the name, or an empty string otherwise
             * (e.g. when the item is a folder).
             */
            wstring extension() const;

            /**
             * @return the path of the item in the archive, if available or inferable from the name, or an empty string
             * otherwise.
             */
            wstring path() const;

            /**
             * @return the uncompressed size of the item.
             */
            uint64_t size() const;

            /**
             * @return the compressed size of the item.
             */
            uint64_t packSize() const;

            /**
             * @return true if and only if the item is encrypted.
             */
            bool isEncrypted() const;

            /**
             * @brief Gets the specified item property.
             *
             * @param property  the property to be retrieved.
             *
             * @return the value of the item property, if available, or an empty BitPropVariant.
             */
            BitPropVariant getProperty( BitProperty property ) const;

            /**
             * @return a map of all the available (i.e. non empty) item properties and their respective values.
             */
            map< BitProperty, BitPropVariant > itemProperties() const;

        private:
            const uint32_t mItemIndex;
            map< BitProperty, BitPropVariant > mItemProperties;

            /* BitArchiveItem objects can be created and updated only by BitArchiveReader */
            explicit BitArchiveItem( uint32_t item_index );

            void setProperty( BitProperty property, const BitPropVariant& value );

            friend class BitArchiveInfo;
    };
}

#endif // BITARCHIVEITEMREADER_HPP
