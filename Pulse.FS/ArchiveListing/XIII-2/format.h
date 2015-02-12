#pragma once

#define ALIGNMENT 2048
#define MAXPATH 1600

struct entry_s
{
	char fileName[MAXPATH]; //Includes path
	unsigned short fileChunk; //Which filechunk has more info about file
	unsigned short fileChunkOffset; //Specific position in filechunk has more info
	unsigned short unknown1; //Two unknown values from flie list entry list
	unsigned short unknown2;
	unsigned short unknown3; //Used for FF13-2 entries
	unsigned int realSize;
	unsigned int compressedSize;
	unsigned int offset; //Offset for compressed file data in archive
	unsigned short offsetMod; //Offset mod for FF13-2 filelists
};

struct fileChunk_s
{
	unsigned int realSize;
	unsigned int compressedSize;
	unsigned int offset;
	char *data;
};

//All compression used is zlib. Everything in filelist (filelist_scrc.win32.bin) seems to be little endian.

/*
Basic structure of filelist file:
- filelist_header_s
- filelist_fileEntry_s * fileNum
- filelist_fileChunkEntry_s * fileChunkNum (not sure whoc file chunk num is calculated, maybe X max amount of files per chunk)
- compressed filechunk data
*/

#pragma pack(push, 1)
struct filelist_header_s
{
	unsigned int fileChunkEntryListOffset;
	unsigned int fileChunksOffset;
	unsigned int fileNum;
};

struct filelist_fileEntry_FF13_s
{
	unsigned short unknown1;
	unsigned short unknown2;
	unsigned short fileChunk; //Which file chunk contains more info about this file
	unsigned short offsetInFileChunk; //Exact position in file chunk with info about this file
};

struct filelist_fileEntry_FF13_2_s //Since this has the same size as filelist_fileEntry_FF13_s the above array is used instead in some areas to define size
{
	unsigned short unknown1;
	unsigned short unknown2;
	unsigned short offsetInFileChunk; //Exact position in file chunk with info about this file. Offsets are funky and doesn't always reset to 0 for each file chunk.
	unsigned short unknown3; //Supposed to be fileChunk? Don't know how it works.
};

struct filelist_fileChunkEntry_s //Info about file chunk
{
	unsigned int realSize;
	unsigned int compressedSize;
	unsigned int offset; //This counts from start of file chunks, not start of file
};
#pragma pack(pop)

/*
Structure of data in uncompressed file chunks:
- File offset as ascii (ends with ':') (value is divided by 2048 before saved to file chunk)
- File real size as ascii (ends with ':')
- File compressed size as ascii (ends with ':')
- Filename (null terminated)

The last filechunk ends with "end\0"
*/

//Actual archive file (white_scrc.win32.bin)is nothing but compressed file data