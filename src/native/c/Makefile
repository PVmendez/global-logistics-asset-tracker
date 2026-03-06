# Health Calculator Library - Makefile
# Compiles ANSI C health calculator library for Linux/GCC

CC = gcc
CFLAGS = -Wall -Wextra -fPIC -O2
LDFLAGS = -shared -lm

# Target
TARGET_LIB = libhealthcalculator.so
TARGET_STATIC = libhealthcalculator.a

# Source files
SOURCES = health_calculator.c
OBJECTS = $(SOURCES:.c=.o)

# Targets
all: $(TARGET_LIB) $(TARGET_STATIC)

$(TARGET_LIB): $(OBJECTS)
	$(CC) $(LDFLAGS) -o $@ $^

$(TARGET_STATIC): $(OBJECTS)
	ar rcs $@ $^

%.o: %.c
	$(CC) $(CFLAGS) -c $< -o $@

clean:
	rm -f $(OBJECTS) $(TARGET_LIB) $(TARGET_STATIC)

.PHONY: all clean
