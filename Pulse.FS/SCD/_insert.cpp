// By blackbird1912
//

#include "stdafx.h"
#include <iostream>
#include <stdio.h>
#include <string.h>
#include <windows.h>
#include <shellapi.h>

using namespace std;

#define SIZE 480
#define PT 255

long filesize(FILE *stream)
{
	long curpos, length;
	curpos = ftell(stream);
	fseek(stream, 0L, SEEK_END);
	length = ftell(stream);
	fseek(stream, curpos, SEEK_SET);
	return length;
}

string revers_chr(char* t){
	string bf1 = "";
	string bf2 = "";
	for (int i = 0; i<6; i++) bf1.push_back('0');
	for (int i = 0; i<7; i++) bf2.push_back(t[i]);
	int b = strlen(bf2.c_str());
	for (int i = 0; i<b; i++) bf1.push_back(t[i]);
	bf2 = "";
	bf2.push_back(bf1[strlen(bf1.c_str()) - 2]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 1]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 4]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 3]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 6]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 5]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 8]);
	bf2.push_back(bf1[strlen(bf1.c_str()) - 7]);
	return bf2;
}

string getChar(char s1, char s2){
	string bufStr = "";
	int n = 0;
	char * p = 0;
	char buk = 0;

	bufStr = s1;
	bufStr = bufStr + s2;
	n = strtol(bufStr.c_str(), &p, 16);
	buk = (char)(n);
	bufStr = buk;
	return bufStr;
}

int _tmain(int argc, _TCHAR* argv[])
{
	LPWSTR *szArglist;
	int nArgs;
	szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
	if (NULL == szArglist){ return 0; }
	char pathToWav[PT] = {0};
	sprintf_s(pathToWav, "%ws", szArglist[1]);
	LocalFree(szArglist);


	FILE *in, *out;//обрезаем 20 байт у wav
	fopen_s(&in, pathToWav, "rb");
	if (NULL == in){ return 0; }
	__int64 x = filesize(in);
	int *file = new int[x], c, i = 0;
	x = 0;
	while ((c = fgetc(in)) != EOF)
	{
		if (x >= 0 && x <= 19){ x++; continue; }
		else { file[i] = c; i++; x++; }
	}
	fclose(in);

	fopen_s(&out,"scd/media.cut", "wb");
	if (NULL == out){ return 0; }
	for (int x = 0; x<i; x++)
	{
		fputc(file[x], out);
	}
	fclose(out);

	int	scdHeaderSize = 480;
	int	bufSize = 100;
	int cutByte = 20;
	__int64 scdSize = x + scdHeaderSize - cutByte;
	__int64 wavSize = x - bufSize - cutByte;

	char hexScdSize[16] = {0};
	char hexWavSize[16] = {0};
	sprintf_s(hexScdSize, "%x", scdSize);
	sprintf_s(hexWavSize, "%x", wavSize);

	string szScd = revers_chr(hexScdSize);
	string szWav = revers_chr(hexWavSize);

	FILE *scdIn, *scdOut;

	fopen_s(&scdIn, "scd.bin", "rb");
	fopen_s(&scdOut, "scd.tmp", "wb");
	if (NULL == scdIn){ return 0; }
	if (NULL == scdOut){ return 0; }

	unsigned char buf[SIZE];
	size_t count = 0;
	char zero[1] = "";
	string tmpStr = "";

	while (count = fread(buf, sizeof(buf[0]), SIZE, scdIn))
	{
		for (int i = 0; i < count; i = i + 4)
		{
			if (i == 16){	//пишем размер scd файла
				if (szScd.c_str()[0] == '0' && szScd.c_str()[1] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szScd.c_str()[0], szScd.c_str()[1]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szScd.c_str()[2] == '0' && szScd.c_str()[3] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szScd.c_str()[2], szScd.c_str()[3]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szScd.c_str()[4] == '0' && szScd.c_str()[5] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szScd.c_str()[4], szScd.c_str()[5]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szScd.c_str()[6] == '0' && szScd.c_str()[7] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szScd.c_str()[6], szScd.c_str()[7]);
					fputs(tmpStr.c_str(), scdOut);
				}
			}
			else if (i == 448){	//пишем размер wav файла
				if (szWav.c_str()[0] == '0' && szWav.c_str()[1] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szWav.c_str()[0], szWav.c_str()[1]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szWav.c_str()[2] == '0' && szWav.c_str()[3] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szWav.c_str()[2], szWav.c_str()[3]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szWav.c_str()[4] == '0' && szWav.c_str()[5] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szWav.c_str()[4], szWav.c_str()[5]);
					fputs(tmpStr.c_str(), scdOut);
				}
				if (szWav.c_str()[6] == '0' && szWav.c_str()[7] == '0'){ fwrite(zero, 1, 1, scdOut); }
				else{
					tmpStr = "";
					tmpStr = getChar(szWav.c_str()[6], szWav.c_str()[7]);
					fputs(tmpStr.c_str(), scdOut);
				}
			}
			else{
				tmpStr = "";
				if (buf[i] != 0){ tmpStr.push_back(buf[i]); fputs(tmpStr.c_str(), scdOut); tmpStr.pop_back(); }
				else{ fwrite(zero, 1, 1, scdOut); }
				if (buf[i + 1] != 0){ tmpStr.push_back(buf[i + 1]); fputs(tmpStr.c_str(), scdOut); tmpStr.pop_back(); }
				else{ fwrite(zero, 1, 1, scdOut); }
				if (buf[i + 2] != 0){ tmpStr.push_back(buf[i + 2]); fputs(tmpStr.c_str(), scdOut); tmpStr.pop_back(); }
				else{ fwrite(zero, 1, 1, scdOut); }
				if (buf[i + 3] != 0){ tmpStr.push_back(buf[i + 3]); fputs(tmpStr.c_str(), scdOut); tmpStr.pop_back(); }
				else{ fwrite(zero, 1, 1, scdOut); }
			}
		}
	}

	fclose(scdIn);
	fclose(scdOut);

	return 0;
}

