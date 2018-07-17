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
GH_RELEASES="$GH_REPO/releases"
AUTH="Authorization: token $github_api_token"
WGET_ARGS="--content-disposition --auth-no-challenge --no-cookie"
CURL_ARGS="-LJO#"

# Validate token.
curl -o /dev/null -sH "$AUTH" $GH_REPO || { echo "Error: Invalid repo, token or network issue!";  exit 1; }

# Read latest release
releasesResponse=$(curl -sH "$AUTH" "$GH_RELEASES/latest")

# Get version of the last release
version=$(echo "$releasesResponse" | grep -oP '(?<="tag_name": ")[^"]*')

echo "Found version: $version"

nextVersion=$(echo $version | awk -F. -v OFS=. 'NF==1{print ++$NF}; NF>1{if(length($NF+1)>length($NF))$(NF-1)++; $NF=sprintf("%0*d", length($NF), ($NF+1)%(10^length($NF))); print}')

echo "Next version: $nextVersion"

# Create a new release
curl -sH "$AUTH" --data "{\"tag_name\":\"$nextVersion\",\"target_commitish\":\"master\",\"name\":\"$nextVersion\",\"body\":\"Release of version $nextVersion. You can deploy this version using https://s3-eu-west-1.amazonaws.com/io.sharksync.builds/$nextVersion/cloudformation.yaml\",\"draft\":false,\"prerelease\":false}" $GH_RELEASES

npm run-script grunt postBuild:$nextVersion