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

#ifndef BITPROPVARIANT_HPP
#define BITPROPVARIANT_HPP

#include <cstdint>
#include <string>
//#include <array>

#include <Propidl.h>

#if _MSC_VER <= 1800
#define NOEXCEPT
#else
#define NOEXCEPT noexcept
#endif

namespace bit7z {

    enum class BitProperty : PROPID {
        NoProperty = 0,         ///<
        MainSubfile,            ///<
        HandlerItemIndex,       ///<
        Path,                   ///<
        Name,                   ///<
        Extension,              ///<
        IsDir,                  ///<
        Size,                   ///<
        PackSize,               ///<
        Attrib,                 ///<
        CTime,                  ///<
        ATime,                  ///<
        MTime,                  ///<
        Solid,                  ///<
        Commented,              ///<
        Encrypted,              ///<
        SplitBefore,            ///<
        SplitAfter,             ///<
        DictionarySize,         ///<
        CRC,                    ///<
        Type,                   ///<
        IsAnti,                 ///<
        Method,                 ///<
        HostOS,                 ///<
        FileSystem,             ///<
        User,                   ///<
        Group,                  ///<
        Block,                  ///<
        Comment,                ///<
        Position,               ///<
        Prefix,                 ///<
        NumSubDirs,             ///<
        NumSubFiles,            ///<
        UnpackVer,              ///<
        Volume,                 ///<
        IsVolume,               ///<
        Offset,                 ///<
        Links,                  ///<
        NumBlocks,              ///<
        NumVolumes,             ///<
        TimeType,               ///<
        Bit64,                  ///<
        BigEndian,              ///<
        Cpu,                    ///<
        PhySize,                ///<
        HeadersSize,            ///<
        Checksum,               ///<
        Characts,               ///<
        Va,                     ///<
        Id,                     ///<
        ShortName,              ///<
        CreatorApp,             ///<
        SectorSize,             ///<
        PosixAttrib,            ///<
        SymLink,                ///<
        Error,                  ///<
        TotalSize,              ///<
        FreeSpace,              ///<
        ClusterSize,            ///<
        VolumeName,             ///<
        LocalName,              ///<
        Provider,               ///<
        NtSecure,               ///<
        IsAltStream,            ///<
        IsAux,                  ///<
        IsDeleted,              ///<
        IsTree,                 ///<
        Sha1,                   ///<
        Sha256,                 ///<
        ErrorType,              ///<
        NumErrors,              ///<
        ErrorFlags,             ///<
        WarningFlags,           ///<
        Warning,                ///<
        NumStreams,             ///<
        NumAltStreams,          ///<
        AltStreamsSize,         ///<
        VirtualSize,            ///<
        UnpackSize,             ///<
        TotalPhySize,           ///<
        VolumeIndex,            ///<
        SubType,                ///<
        ShortComment,           ///<
        CodePage,               ///<
        IsNotArcType,           ///<
        PhySizeCantBeDetected,  ///<
        ZerosTailIsAllowed,     ///<
        TailSize,               ///<
        EmbeddedStubSize,       ///<
        NtReparse,              ///<
        HardLink,               ///<
        INode,                  ///<
        StreamId,               ///<
        ReadOnly,               ///<
        OutName,                ///<
        CopyLink                ///<
    };

    using std::wstring;
    //using std::array;

    /*static const array < wstring, static_cast< int >( BitProperty::CopyLink ) + 1 > propertyNames = {
        L"NoProperty",
        L"MainSubfile",
        L"HandlerItemIndex",
        L"Path",
        L"Name",
        L"Extension",
        L"IsDir",
        L"Size",
        L"PackSize",
        L"Attrib",
        L"CTime",
        L"ATime",
        L"MTime",
        L"Solid",
        L"Commented",
        L"Encrypted",
        L"SplitBefore",
        L"SplitAfter",
        L"DictionarySize",
        L"CRC",
        L"Type",
        L"IsAnti",
        L"Method",
        L"HostOS",
        L"FileSystem",
        L"User",
        L"Group",
        L"Block",
        L"Comment",
        L"Position",
        L"Prefix",
        L"NumSubDirs",
        L"NumSubFiles",
        L"UnpackVer",
        L"Volume",
        L"IsVolume",
        L"Offset",
        L"Links",
        L"NumBlocks",
        L"NumVolumes",
        L"TimeType",
        L"Bit64",
        L"BigEndian",
        L"Cpu",
        L"PhySize",
        L"HeadersSize",
        L"Checksum",
        L"Characts",
        L"Va",
        L"Id",
        L"ShortName",
        L"CreatorApp",
        L"SectorSize",
        L"PosixAttrib",
        L"SymLink",
        L"Error",
        L"TotalSize",
        L"FreeSpace",
        L"ClusterSize",
        L"VolumeName",
        L"LocalName",
        L"Provider",
        L"NtSecure",
        L"IsAltStream",
        L"IsAux",
        L"IsDeleted",
        L"IsTree",
        L"Sha1",
        L"Sha256",
        L"ErrorType",
        L"NumErrors",
        L"ErrorFlags",
        L"WarningFlags",
        L"Warning",
        L"NumStreams",
        L"NumAltStreams",
        L"AltStreamsSize",
        L"VirtualSize",
        L"UnpackSize",
        L"TotalPhySize",
        L"VolumeIndex",
        L"SubType",
        L"ShortComment",
        L"CodePage",
        L"IsNotArcType",
        L"PhySizeCantBeDetected",
        L"ZerosTailIsAllowed",
        L"TailSize",
        L"EmbeddedStubSize",
        L"NtReparse",
        L"HardLink",
        L"INode",
        L"StreamId",
        L"ReadOnly",
        L"OutName",
        L"CopyLink"
    };*/

    enum class BitPropVariantType : uint32_t {
        Empty,      ///< Empty BitPropVariant type
        Bool,       ///< Boolean BitPropVariant type
        String,     ///< String BitPropVariant type
        UInt8,      ///< 8-bit unsigned int BitPropVariant type
        UInt16,     ///< 16-bit unsigned int BitPropVariant type
        UInt32,     ///< 32-bit unsigned int BitPropVariant type
        UInt64,     ///< 64-bit unsigned int BitPropVariant type
        Int8,       ///< 8-bit signed int BitPropVariant type
        Int16,      ///< 16-bit signed int BitPropVariant type
        Int32,      ///< 32-bit signed int BitPropVariant type
        Int64,      ///< 64-bit signed int BitPropVariant type
        Filetime    ///< FILETIME BitPropVariant type
    };

    /*static const array < wstring, static_cast< uint32_t >( BitPropVariantType::Filetime ) + 1 > typeNames = {
        L"Empty",
        L"Bool",
        L"String",
        L"UInt8",
        L"UInt16",
        L"UInt32",
        L"UInt64",
        L"Int8",
        L"Int16",
        L"Int32",
        L"Int64",
        L"Filetime"
    };*/

    /**
     * @brief The BitPropVariant struct is a light extension to the WinAPI PROPVARIANT struct providing useful getters.
     */
    struct BitPropVariant : public PROPVARIANT {
            /**
             * @brief Constructs an empty BitPropVariant object.
             */
            BitPropVariant();

            /**
             * @brief Copy constructs this BitPropVariant from another one.
             *
             * @param other the variant to be copied.
             */
            BitPropVariant( const BitPropVariant& other );

            /**
             * @brief Move constructs this BitPropVariant from another one.
             *
             * @param other the variant to be moved.
             */
            BitPropVariant( BitPropVariant&& other ) NOEXCEPT;

            /**
             * @brief Constructs a boolean BitPropVariant
             *
             * @param value the bool value of the BitPropVariant
             */
            explicit BitPropVariant( bool value );

            /**
             * @brief Constructs a string BitPropVariant from a null-terminated C wide string
             *
             * @param value the null-terminated C wide string value of the BitPropVariant
             */
            explicit BitPropVariant( const wchar_t* value );

            /**
             * @brief Constructs a string BitPropVariant from a wstring
             *
             * @param value the wstring value of the BitPropVariant
             */
            explicit BitPropVariant( const wstring& value );

            /**
             * @brief Constructs a 8-bit unsigned integer BitPropVariant
             *
             * @param value the uint8_t value of the BitPropVariant
             */
            explicit BitPropVariant( uint8_t value );

            /**
             * @brief Constructs a 16-bit unsigned integer BitPropVariant
             *
             * @param value the uint16_t value of the BitPropVariant
             */
            explicit BitPropVariant( uint16_t value );

            /**
             * @brief Constructs a 32-bit unsigned integer BitPropVariant
             *
             * @param value the uint32_t value of the BitPropVariant
             */
            explicit BitPropVariant( uint32_t value );

            /**
             * @brief Constructs a 64-bit unsigned integer BitPropVariant
             *
             * @param value the uint64_t value of the BitPropVariant
             */
            explicit BitPropVariant( uint64_t value );

            /**
             * @brief Constructs a 8-bit integer BitPropVariant
             *
             * @param value the int8_t value of the BitPropVariant
             */
            explicit BitPropVariant( int8_t value );

            /**
             * @brief Constructs a 16-bit integer BitPropVariant
             *
             * @param value the int16_t value of the BitPropVariant
             */
            explicit BitPropVariant( int16_t value );

            /**
             * @brief Constructs a 32-bit integer BitPropVariant
             *
             * @param value the int32_t value of the BitPropVariant
             */
            explicit BitPropVariant( int32_t value );

            /**
             * @brief Constructs a 64-bit integer BitPropVariant
             *
             * @param value the int64_t value of the BitPropVariant
             */
            explicit BitPropVariant( int64_t value );

            /**
             * @brief Constructs a FILETIME BitPropVariant
             *
             * @param value the FILETIME value of the BitPropVariant
             */
            explicit BitPropVariant( const FILETIME& value );

            /**
             * @brief BitPropVariant destructor.
             *
             * @note This is not virtual, in order to maintain the same memory layout of the base struct!
             */
            ~BitPropVariant();

            /**
             * @brief Copy assignment operator.
             *
             * @param other the variant to be copied.
             *
             * @return a reference to *this object (with the copied values from other).
             */
            BitPropVariant& operator=( const BitPropVariant& other ) NOEXCEPT;

            /**
             * @brief Move assignment operator.
             *
             * @param other the variant to be moved.
             *
             * @return a reference to *this object (with the moved values from other).
             */
            BitPropVariant& operator=( BitPropVariant&& other ) NOEXCEPT;

            /**
             * @brief Assignment operator
             *
             * @note this will work only for T types for which a BitPropVariant constructor is defined!
             *
             * @param value the value to be assigned to the object
             *
             * @return a reference to *this object having the value as new variant value
             */
            template<typename T>
            BitPropVariant& operator=( const T& value ) {
                *this = BitPropVariant( value );
                return *this;
            }

            /**
             * @return the boolean value of this variant
             * (it throws an expcetion if the variant is not a boolean).
             */
            bool getBool() const;

            /**
             * @return the string value of this variant
             * (it throws an exception if the variant is not a string).
             */
            wstring getString() const;

            /**
             * @return the 8-bit unsigned integer value of this variant
             * (it throws an exception if the variant is not an 8-bit unsigned integer).
             */
            uint8_t getUInt8() const;

            /**
             * @return the 16-bit unsigned integer value of this variant
             * (it throws an exception if the variant is not an 8 or 16-bit unsigned integer).
             */
            uint16_t getUInt16() const;

            /**
             * @return the 32-bit unsigned integer value of this variant
             * (it throws an exception if the variant is not an 8, 16 or 32-bit unsigned integer).
             */
            uint32_t getUInt32() const;

            /**
             * @return the 64-bit unsigned integer value of this variant
             * (it throws an exception if the variant is not an 8, 16, 32 or 64-bit unsigned integer).
             */
            uint64_t getUInt64() const;

            /**
             * @return the 8-bit integer value of this variant
             * (it throws an exception if the variant is not an 8-bit integer).
             */
            int8_t getInt8() const;

            /**
             * @return the 16-bit integer value of this variant
             * (it throws an exception if the variant is not an 8 or 16-bit integer).
             */
            int16_t getInt16() const;

            /**
             * @return the 32-bit integer value of this variant
             * (it throws an exception if the variant is not an 8, 16 or 32-bit integer).
             */
            int32_t getInt32() const;

            /**
             * @return the 64-bit integer value of this variant
             * (it throws an exception if the variant is not an 8, 16, 32 or 64-bit integer).
             */
            int64_t getInt64() const;

            /**
             * @return the FILETIME value of this variant
             * (it throws an exception if the variant is not a filetime).
             */
            FILETIME getFiletime() const;

            /**
             * @return the the value of this variant converted from any supported type to std::wstring.
             */
            wstring toString() const;

            /**
             * @return true if this variant is empty, false otherwise.
             */
            bool isEmpty() const;

            /**
             * @return true if this variant is a boolean, false otherwise.
             */
            bool isBool() const;

            /**
             * @return true if this variant is a string, false otherwise.
             */
            bool isString() const;

            /**
             * @return true if this variant is an 8-bit unsigned integer, false otherwise.
             */
            bool isUInt8() const;

            /**
             * @return true if this variant is an 8 or 16-bit unsigned integer, false otherwise.
             */
            bool isUInt16() const;

            /**
             * @return true if this variant is an 8, 16 or 32-bit unsigned integer, false otherwise.
             */
            bool isUInt32() const;

            /**
             * @return true if this variant is an 8, 16, 32 or 64-bit unsigned integer, false otherwise.
             */
            bool isUInt64() const;

            /**
             * @return true if this variant is an 8-bit integer, false otherwise.
             */
            bool isInt8() const;

            /**
             * @return true if this variant is an 8 or 16-bit integer, false otherwise.
             */
            bool isInt16() const;

            /**
             * @return true if this variant is an 8, 16 or 32-bit integer, false otherwise.
             */
            bool isInt32() const;

            /**
             * @return true if this variant is an 8, 16, 32 or 64-bit integer, false otherwise.
             */
            bool isInt64() const;

            /**
             * @return true if this variant is a FILETIME structure, false otherwise.
             */
            bool isFiletime() const;

            /**
             * @return the BitPropVariantType of this variant.
             */
            BitPropVariantType type() const;

            /**
             * @brief Clears the current value of the variant object
             */
            void clear();

        private:
            void internalClear();

            friend bool operator ==( const BitPropVariant& a, const BitPropVariant& b );
            friend bool operator !=( const BitPropVariant& a, const BitPropVariant& b );
    };
}

#endif // BITPROPVARIANT_HPP
