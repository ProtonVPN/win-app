
## Profiles API

 ### Profiles API fields
   - Color:
     - API accepts predefined list of values. Call GET /vpn/profiles/colors for predefined lis of values;
     - value is accepted both in lowercase and uppercase, but it is always returned in lowercase;
   - Name:
     - uniqueness check is case insensitive;
     - API removes leading and trailing spaces before processing;
   - Protocol:
     - 1 = OpenVPN (TCP);
     - 2 = OpenVPN (UDP);
     - 3 = Smart;
     - 4 = IKEv2.

 ### Profiles API response codes
   - 86062. Invalid profile ID (when updating)
   - 86063. Invalid profile ID (when deleting)
   - 86065. A profile with this name already exists (when creating or updating)

See [API response codes](https://docs.google.com/spreadsheets/d/1-uX3j4VisF0ZcS3K3OB9Mg5U737jX3A3q4sqY47rBdQ/edit#gid=0)
 
 - Synchronization approuch used in ProtonVPN Windows app guarantees:
   - availability (every request receives a non-error response – without the guarantee that it contains the most recent write),
   - partition tolerance (the system continues to operate despite an arbitrary number of messages being dropped or delayed by the network between nodes),
   - but not consistency (every read receives the most recent write or an error).

 - Protocol synchronization principles:
   - Uses asyncronous syncronization with no user intervention.
   - Maps not supported protocol values coming from API to Smart protocol befor any other processing.
   - Automatically renames new or updated profiles to ensure profile names are unique. A space and a number is added to the end of the profile name. The number is increased each time the duplicate name is detected.
   - Uses "last writer wins" synchronization approuch during first 5 min after local profile change:
     - (updated or deleted) updates or deletes profile regardless if it was changed on the API by another session;
     - (updated) creates new profile if it was removed from the server by another session;
   - Uses "first writer wins" synchronization approuch after first 5 min after local profile chnage:
     - (updated or deleted) keeps profile version from the API if profile has changed in the API after local profile change. The time of last profile change isn't tracked on the API. It is assumed profile last change occured when the changed profile was received from the API;
   - Merges locally created profiles with new profiles coming from the API during synchronization if their properties match (profile color is ignored during comparison).

 - The profile synchronization implementation:
   - Uses local three layer profile cache:
     - Local. Locally changed profiles are saved to this layer;
     - Sync. The working list of changed profiles for the synchronization process;
     - External. Copy of profiles retrieved from the API;

   - Cached profile properties important for synchronization:
     - Id. The locally generated unique identifier. Mandatory for all cached profiles.
     - ExternalId. The API generated unique identifier. Empty for locally created and not yet synchronized with the API profiles.
     - Status. Possible values:
       - Synced.
       - Created.
       - Updated.
       - Deleted.
     - SyncStatus. Used for displaying purposes only. Possible vlues:
       - InProgress. The synchronization has not yet started or is in progress.
       - Succeeded. The synchronization has succeeded.
       - Failed. The synchronization has failed.
       - Overridden. The local change was overridden by the one coming from the API.
     - ModifiedAt. The UTC date and time of last profile modification.
     - OriginalName. The profile name originally added by the user.
     - UniqueNameIndex. The numeric index used to generate the uniue profile name.

   - Profiles in the External cache layer always have Synced status value. Profiles in the Local and Sync cache layers have one of other status values (Created, Updated or Deleted).

   - Before starting synchronizaton for the first time all old saved profiles are added to the Local profile cache as if they were created very long time ago but are not yet synchronized.

   - Retrieving list of profiles from the cache:
     - Lock profile cache.
       - Union the profiles from the Local cache layer with the Sync cache layer and the External cache layer comparing profiles by ID. Profile which comes first is kept in the unioned list.
     - Filter out deleted profiles.
     - Append predefined profiles. Predefined profiles are kept outside of the profile cache.

   - Saving local changes to the profile cache:
     - Add:
       - Set synchronization status of incoming profile to InProgress.
       - Set modification date of incoming profile to now.
       - Set status of incoming profile to Created.
       - Lock profile cache.
         - Add incoming profile to Local cache layer. 
     - Update:
       - Set synchronization status of incoming profile to InProgress.
       - Set modification date of incoming profile to now.
       - Lock profile cache.
         - Find corresponding profile in the Local cache layer comparing by ID.
         - If found:
           - Copy status value from the found profile.
           - Replace cached profile with the incoming one.
         - If not found:
           - Set status of incoming profile to Updated.
           - Add incoming profile to the Local cache layer.
     - Delete:
       - Set synchronization status of incoming profile to InProgress.
       - Set modification date of incoming profile to now.
       - Lock profile cache.
         - Find corresponding profile in the Local cache layer comparing by ID.
         - If found:
           - If status value of the found profile is Created:
             - Remove the found profile from Local cache layer.
           - If status value of the found profile is not Created:
             - Set status of incoming profile to Deleted.
             - Replace cached profile with the incoming one.
         - If not found:
           - Set status of incoming profile to Deleted.
           - Add incoming profile to the Local cache layer.

   - The steps of the synchronization process:
     - 1. Merge profiles retrieved from API into External cache layer:
       - Retrieve a list of all profiles from the API.
       - Merge incoming profiles:
         - Lock profile cache.
           - Process incoming profiles in a loop:
             - Find corresponding profile in the External cache layer comparing by ExternalID.
             - If found:
               - Compare all properties of the incoming profile with the cached one.
               - If they match, ignore the incoming profile.
               - If they don't match:
                 - Set status of incoming profile to Synced.
                 - Set modification date of incoming profile to now.
                 - Copy ID value from the cached profile.
                 - Replace cached profile with the incoming one.
             - If not found:
               - If merging new duplicate profiles is supported:
                 - ...
       - Remove profiles with ExternalID values not in the incoming list from the External cache layer.

     - 2. Merge profiles from Local cache layer to Sync cache layer:
       - Lock profile cache.
         - Process Local cache layer profiles in a loop:
           - Find corresponding profile in the Sync cache layer comparing by ID.
           - If found:
             - Copy status value from the found profile.
             - Replace the found profile.
           - If not found:
             - Add profile to the Sync cache layer.
         - Clear Local cache layer.

     - 3. Send profile changes from Sync cache layer to the API:
       - Lock profile cache.
         - Get profiles from Sync cache layer.
       - Order profiles by the modification date.
       - Process profiles in a loop:

         - If profile status is Created:
           - Ensure profile name uniqueness.
           - Create new profile in the API, copy properties returned from the API.
           - If a profile with this name already exists in the API:
             - Update profile with next unique name candidate.
             - Lock profile cache.
               - Replace corresponding profile in Sync cache layer comparing by ID.
           - If succeeded:
             - Set status to Synced.
             - Set synchronization status to Succeeded.
             - Lock profile cache.
               - Add or replace corresponding profile in External cache layer comparing by ID.
               - Remove profile from Sync cache layer;

         - If profile status is Updated:
           - Find corresponding profile in the External cache layer comparing by ID.
           - If not found:
             - Profile was deleted while editing or syncing.
             - If forced profile synchronization interval has elapsed after profile changed:
               - First wins: Skip this profile:
                 - Lock profile cache.
                   - Remove profile from Sync cache layer;
             - If forced profile synchronization interval has not elapsed after profile changed:
               - Last wins: Create new profile:
                 - Goto "If profile status is Created:".
           - If found:
             - Copy ExternalID from the found profile.
             - If the found profile is modified later than the profile being synced:
               - If forced profile synchronization interval has elapsed after profile changed: 
                 - Profile was updated while syncing.
                 - First wins: Skip this profile:
                   - Set status to Synced.
                   - Set synchronization status to Overridden.
                   - Lock profile cache.
                     - Replace corresponding profile in External cache layer comparing by ID.
                     - Remove profile from Sync cache layer;
                 - Continue to next profile in the loop;
             - Update profile with unique name.
             - 

             - ...

         - If profile status is Deleted:
           - ...


 - Server change/deletion problem:
   - what to do if server was deleted or server features changed and do not match profile type? Windows client does nothing in this case;
     - silently delete the profile. Doesn't look releable;
     - automatically change profile type to match changed server features. Doesn't look releable;
     - display profile dimmed with the message describing the problem in the profile window. Don't allow connecting. Allow deleting or changing profile (selecting new server with features matching profile type);
   - Note: the profile type can be selected when creating new profile but canot be changed later.
