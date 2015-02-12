#include "misc.h"
#include "h.h"

void MakeDirectory(char *fullpath)
{
	int pos = 0;
	char path[MAKEDIR_MAXSIZE];
	memset(path, 0, MAKEDIR_MAXSIZE);

	while(1)
	{
		if(fullpath[pos] == 0)
			return;
		path[pos] = fullpath[pos];
		pos++;
		if(fullpath[pos] == '\\' || fullpath[pos] == '/')
			_mkdir(path);
	}
}

void DataToHex(char *to, char *from, int size) 
{
	char const hextable[] = {'0','1','2','3','4','5','6','7','8','9','a','b','c','d','e','f'};

	int i, j = 0;
	for(i = 0; i < size; i++)
	{
		to[j] = hextable[from[i] >> 4 & 0xf];
		to[j + 1] = hextable[from[i] & 0xf];
		j += 2;
	}
}

unsigned char ChartoHex(char *bytes, int offset)
{
	unsigned char value = 0;
	unsigned char finValue = 0;

	for(int i = 0; i < 2; i++)
	{
		char c = bytes[i + offset];
		if( c >= '0' && c <= '9' )
			value = c - '0';
        else if( c >= 'A' && c <= 'F' )
            value = 10 + (c - 'A');
        else if( c >= 'a' && c <= 'f' )
            value = 10 + (c - 'a');

		if(i == 0 && value > 0)
			value *= 16;

		finValue += value;
	}
	return finValue;
}

//Variant which is limited by string size, rather than value size
void CharByteToData_Beta(char *num, unsigned char *c, int size)
{
	//Prepare string
	char num2[100];
	int pos = 0;
	int pos2 = 0;
	int strSize = 0;
	while(1)
	{
		if(num[strSize] == 0)
			break;
		strSize++;
	}
	if(strSize % 2) //If string has odd length, then add a zero to make this conversion work properly
	{
		num2[0] = '0';
		pos2++;
		strSize++;
	}
	while(1)
	{
		num2[pos2] = num[pos];
		if(num[pos] == 0)
			break;
		pos++;
		pos2++;
	}

	//Actual conversion
	int i = size - 1 - ( ((size * 2) - strSize) / 2);
	for(int j = 0; j < strSize; j += 2)
	{
		c[i] = ChartoHex(num2, j);
		i--;
	}
}

void CharByteToData(char *num, unsigned char *c, int size) //Note, num size has to be twice the size of "size" otherwise this function fails (in which case, use above function)
{
	int j = 0;
	for(int i = size - 1; i > -1; i--)
	{
		c[i] = ChartoHex(num, j);
		j += 2;
	}
}