#!/bin/bash

PY_FILES=$(
    find . -name '*.py' \
    | grep "^\./ZenPacks/community/HuaweiServer/"
)
pep8 --max-line-length=80 --show-source $PY_FILES
