#include "pch.h"
#include "matcher.h"

namespace ipfilter
{
    namespace matcher
    {
        Matcher::Matcher(FWP_MATCH_TYPE type): type(type)
        {
        }

        Matcher::operator FWP_MATCH_TYPE()
        {
            return this->type;
        }

        Matcher equal()
        {
            return Matcher(FWP_MATCH_EQUAL);
        }

        Matcher notEqual()
        {
            return Matcher(FWP_MATCH_NOT_EQUAL);
        }
    }
}
