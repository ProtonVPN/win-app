#pragma once
#include <fwptypes.h>

namespace ipfilter
{
    namespace matcher
    {
        class Matcher
        {
        public:
            Matcher(FWP_MATCH_TYPE type);

            virtual operator FWP_MATCH_TYPE();

        private:
            FWP_MATCH_TYPE type;
        };

        Matcher equal();

        Matcher notEqual();
    }
}
