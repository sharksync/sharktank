#!/bin/bash
for file in bin/release/netcoreapp2.0/*.zip
do
  if [ -f "$file" ];then
    newfile=$(openssl sha1 $file | awk '{print $2}')
    cp $file $newfile.zip
    export LAMBDA_FILE="$newfile.zip"
  fi
done