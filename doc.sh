#!/bin/sh
groff -W number -k -Tutf8 -dpaper=a4 -man micro-config.3 | less -r
