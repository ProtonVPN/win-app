#include "pch.h"
#include "EnvironmentVariable.h"

std::string GetEnvironmentVariable(const char* environmentVariableKey)
{
    char* value;
    size_t length;
    const errno_t error = _dupenv_s(&value, &length, environmentVariableKey);
    std::string result;
    if (value != nullptr && !error)
    {
        result = value;
    }
    return result;
}