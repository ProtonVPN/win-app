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

#ifndef BITEXCEPTION_HPP
#define BITEXCEPTION_HPP

#include <string>
#include <stdexcept>

#include <Windows.h>

namespace bit7z {
    using std::runtime_error;
    using std::wstring;

    /**
     * @brief The BitException class represents a generic exception thrown from the bit7z classes.
     */
    class BitException : public runtime_error {
        public:
            /**
             * @brief Constructs a BitException object with the given message.
             *
             * @param message   the message associated with the exception object.
             * @param code      the HRESULT code associated with the exception object.
             */
            explicit BitException( const char* message, HRESULT code = E_FAIL );

            /**
             * @brief Constructs a BitException object with the given message.
             *
             * @note The Win32 error code is converted to a HRESULT code through HRESULT_FROM_WIN32 macro.
             *
             * @param message   the message associated with the exception object.
             * @param code      the Win32 error code associated with the exception object.
             */
            BitException( const char* message, DWORD code );

            /**
             * @brief Constructs a BitException object with the given message.
             *
             * @note The wstring argument is converted into a string and then passed to the base
             * class constructor.
             *
             * @param message   the message associated with the exception object.
             * @param code      the HRESULT code associated with the exception object.
             */
            explicit BitException( const wstring& message, HRESULT code = E_FAIL );

            /**
             * @brief Constructs a BitException object with the given message.
             *
             * @note The wstring argument is converted into a string and then passed to the base
             * class constructor.
             *
             * @note The Win32 error code is converted to a HRESULT code through HRESULT_FROM_WIN32 macro.
             *
             * @param message   the message associated with the exception object.
             * @param code      the Win32 error code associated with the exception object.
             */
            BitException( const wstring& message, DWORD code );

            /**
             * @return the HRESULT code associated with the exception object.
             */
            HRESULT getErrorCode();

        private:
            HRESULT mErrorCode;
    };
}
#endif // BITEXCEPTION_HPP
