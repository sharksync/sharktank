#!/usr/bin/env bash

# Check dependencies.
set -e
xargs=$(which gxargs || which xargs)

# Validate settings.
[ "$TRACE" ] && set -x

CONFIG=$@

for line in $CONFIG; do
  eval "$line"
done

# Define variables.
GH_API="https://api.github.com"
GH_REPO="$GH_API/repos/$owner/$repo"
GH_RELEASES="$GH_REPO/releases/latest"
AUTH="Authorization: token $github_api_token"
WGET_ARGS="--content-disposition --auth-no-challenge --no-cookie"
CURL_ARGS="-LJO#"

# Validate token.
curl -o /dev/null -sH "$AUTH" $GH_REPO || { echo "Error: Invalid repo, token or network issue!";  exit 1; }

# Read latest release
releasesResponse=$(curl -sH "$AUTH" $GH_RELEASES)

# Get version of the last release
echo "$releasesResponse" | grep -m 1 "name.:"
eval $(echo "$releasesResponse" | grep -m 1 "name.:" | grep -w name | tr : = | tr -cd '[[:alnum:]]=')
#[ "$version" ] || { echo "Error: Failed to get release version for latest release"; echo "$releasesResponse" | awk 'length($0)<100' >&2; exit 1; }

echo "Found version: $version"

$nextVersion = (echo $version | awk -F. -v OFS=. 'NF==1{print ++$NF}; NF>1{if(length($NF+1)>length($NF))$(NF-1)++; $NF=sprintf("%0*d", length($NF), ($NF+1)%(10^length($NF))); print}')

echo "Moved to version: $nextVersion"

# Create a new release
curl -sH "$AUTH" --data '{"tag_name":"$nextVersion","target_commitish":"master","name":"$nextVersion","body":"Release of version $nextVersion","draft":false,"prerelease":false}' $GH_RELEASES

# Read latest release
releasesResponse=$(curl -sH "$AUTH" $GH_RELEASES)

# Get ID of the release
eval $(echo "$releasesResponse" | grep -m 1 "id.:" | grep -w id | tr : = | tr -cd '[[:alnum:]]=')
[ "$id" ] || { echo "Error: Failed to get release id for latest release"; echo "$releasesResponse" | awk 'length($0)<100' >&2; exit 1; }

# Upload asset
echo "Uploading asset... "

# Construct url
GH_ASSET="https://uploads.github.com/repos/$owner/$repo/releases/$id/assets?name=$(basename $filename)"

curl "$GITHUB_OAUTH_BASIC" --data-binary @"$filename" -H "$AUTH" -H "Content-Type: application/octet-stream" $GH_ASSET