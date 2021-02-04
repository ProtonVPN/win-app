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

#ifndef BIT7ZLIBRARY_HPP
#define BIT7ZLIBRARY_HPP

#include <string>

#include <Windows.h>

#define DEFAULT_DLL L"7z.dll"

struct IInArchive;
struct IOutArchive;

//! \cond IGNORE_BLOCK_IN_DOXYGEN
template< typename T >
class CMyComPtr;
//! \endcond

namespace bit7z {
    /**
     * @brief The Bit7zLibrary class allows the access to the basic functionalities provided by the 7z DLLs.
     */
    class Bit7zLibrary {
        public:
            /**
             * @brief Constructs a Bit7zLibrary object using the path of the wanted 7zip DLL.
             *
             * By default, it searches a 7z.dll in the same path of the application.
             *
             * @param dll_path  the path to the dll wanted
             */
            explicit Bit7zLibrary( const std::wstring& dll_path = DEFAULT_DLL );

            /**
             * @brief Destructs the Bit7zLibrary object, freeing the loaded dynamic-link library (DLL) module.
             */
            virtual ~Bit7zLibrary();

            /**
             * @brief Initiates the object needed to create a new archive or use an old one.
             *
             * @note Usually this method should not be called directly by users of the bit7z library.
             *
             * @param format_ID     GUID of the archive format (see BitInFormat's guid() method)
             * @param interface_ID  ID of the archive interface to be requested (IID_IInArchive or IID_IOutArchive)
             * @param out_object    Pointer to a CMyComPtr of an object wich implements the interface requested
             */
            void createArchiveObject( const GUID* format_ID, const GUID* interface_ID, void** out_object ) const;

            /**
             * @brief Set the 7-zip dll to use large memory pages.
             */
            void setLargePageMode();

        private:
            typedef UINT32 ( WINAPI* CreateObjectFunc )( const GUID* clsID, const GUID* interfaceID, void** out );
            typedef HRESULT ( WINAPI* SetLargePageMode )();

            HMODULE mLibrary;
            CreateObjectFunc mCreateObjectFunc;

            Bit7zLibrary( const Bit7zLibrary& ); // not copyable!
            Bit7zLibrary& operator=( const Bit7zLibrary& ); // not assignable!
    };
}

#endif // BIT7ZLIBRARY_HPP
