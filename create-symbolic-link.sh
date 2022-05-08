#!/ush/bash

DIR="$HOME/.local/bin"
TARGET=`pwd`/build/notsc/Coven
if [ -d $DIR ]; then
    echo "Creating a symbolic link to '$TARGET' with name '$DIR/coven'"
    ln -f -s "$TARGET" "$DIR/coven"
else
    echo "The directory '$DIR' does not exist"
fi
