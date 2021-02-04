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

#ifndef BITARCHIVEHANDLER_HPP
#define BITARCHIVEHANDLER_HPP

#include <cstdint>
#include <functional>

#include "../include/bit7zlibrary.hpp"

namespace bit7z {
    using std::wstring;
    using std::function;

    class BitInFormat;

    /**
     * @brief A std::function whose argument is the total size of the ongoing operation.
     */
    typedef function< void( uint64_t total_size ) > TotalCallback;

    /**
     * @brief A std::function whose argument is the current processed size of the ongoing operation.
     */
    typedef function< void( uint64_t progress_size ) > ProgressCallback;

    /**
     * @brief A std::function whose arguments are the current processed input size and the current output size of the
     * ongoing operation.
     */
    typedef function< void( uint64_t input_size, uint64_t output_size ) > RatioCallback;

    /**
     * @brief A std::function whose argument is the name of the file currently being processed by the ongoing operation.
     */
    typedef function< void( wstring filename ) > FileCallback;

    /**
     * @brief A std::function which returns the password to be used in order to handle an archive.
     */
    typedef function< wstring() > PasswordCallback;

    /**
     * @brief Abstract class representing a generic archive handler.
     */
    class BitArchiveHandler {
        public:
            /**
             * @return the Bit7zLibrary object used by the handler.
             */
            const Bit7zLibrary& library() const;

            /**
             * @return the format used by the handler for extracting or compressing.
             */
            virtual const BitInFormat& format() const = 0;

            /**
             * @return the password used to open, extract or encrypt the archive.
             */
            const wstring password() const;

            /**
             * @return true if a password is defined, false otherwise.
             */
            bool isPasswordDefined() const;

            /**
             * @return the current total callback.
             */
            TotalCallback totalCallback() const;

            /**
             * @return the current progress callback.
             */
            ProgressCallback progressCallback() const;

            /**
             * @return the current ratio callback.
             */
            RatioCallback ratioCallback() const;

            /**
             * @return the current file callback.
             */
            FileCallback fileCallback() const;

            /**
             * @return the current password callback.
             */
            PasswordCallback passwordCallback() const;

            /**
             * @brief Sets up a password to be used by the archive handler.
             *
             * The password will be used to encrypt/decrypt archives by using the default
             * cryptographic method of the archive format.
             *
             * @note Calling setPassword when the input archive is not encrypted does not have effect on
             * the extraction process.
             *
             * @note Calling setPassword when the output format doesn't support archive encryption
             * (e.g. GZip, BZip2, etc...) does not have any effects (in other words, it doesn't
             * throw exceptions and it has no effects on compression operations).
             *
             * @note After a password has been set, it will be used for every subsequent operation.
             * To disable the use of the password, you need to call the clearPassword method, which is equivalent
             * to call setPassword(L"").
             *
             * @param password  the password to be used.
             */
            virtual void setPassword( const wstring& password );

            /**
             * @brief Clear the current password used by the handler.
             *
             * Calling clearPassword() will disable the encryption/decryption of archives.
             *
             * @note This is equivalent to calling setPassword(L"").
             */
            void clearPassword();

            /**
             * @brief Sets the callback to be called when the total size of an operation is available.
             *
             * @param callback  the total callback to be used.
             */
            void setTotalCallback( const TotalCallback& callback );

            /**
             * @brief Sets the callback to be called when the processed size of the ongoing operation is updated.
             *
             * @note The percentage of completition of the current operation can be obtained by calculating
             * static_cast<int>( ( 100.0 * processed_size ) / total_size ).
             *
             * @param callback  the progress callback to be used.
             */
            void setProgressCallback( const ProgressCallback& callback );

            /**
             * @brief Sets the callback to be called when the input processed size and current output size of the
             * ongoing operation are known.
             *
             * @note The ratio percentage of a compression operation can be obtained by calculating
             * static_cast<int>( ( 100.0 * output_size ) / input_size ).
             *
             * @param callback  the ratio callback to be used.
             */
            void setRatioCallback( const RatioCallback& callback );

            /**
             * @brief Sets the callback to be called when the currently file being processed changes.
             *
             * @param callback  the file callback to be used.
             */
            void setFileCallback( const FileCallback& callback );

            /**
             * @brief Sets the callback to be called when a password is needed to complete the ongoing operation.
             *
             * @param callback  the password callback to be used.
             */
            void setPasswordCallback( const PasswordCallback& callback );

        protected:
            const Bit7zLibrary& mLibrary;
            wstring mPassword;

            explicit BitArchiveHandler( const Bit7zLibrary& lib, const wstring& password = L"" );

            virtual ~BitArchiveHandler() = 0;

        private:
            //CALLBACKS
            TotalCallback mTotalCallback;
            ProgressCallback mProgressCallback;
            RatioCallback mRatioCallback;
            FileCallback mFileCallback;
            PasswordCallback mPasswordCallback;
    };
}

#endif // BITARCHIVEHANDLER_HPP
