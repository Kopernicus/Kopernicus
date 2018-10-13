#!/bin/bash

KOPERNICUS=$1
BACKPORT=$2

find $BACKPORT -type f -not -path "*.git*" | while read file
do 
    path=${file#"$BACKPORT/"}
    diff -u $KOPERNICUS/$path $BACKPORT/$path > $BACKPORT/$path.patch 
done
