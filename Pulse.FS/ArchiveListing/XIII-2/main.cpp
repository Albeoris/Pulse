#include "h.h"
#include "main.h"
#include "format.h"
#include "help-text.h"
#include "easyzlib/easyzlib.h"
#include "misc.h"
#include "log.h"

#define EZ_COMPRESSMAXDESTLENGTH(n) (n+(((n)/1000)+1)+12)

FILE *fileList, *fileArchive;
unsigned int fileListSize;
entry_s *entries;
unsigned int fileNum = 0;
short chunkNum = 0;
fileChunk_s *fileChunks;
int mode;
bool decrypted = 0; //Whether or not we're dealing with a decrypted file
unsigned char cryptHeader[32]; //Encryption heaer
int game; //What game this file is from

void ConvertIntToHexString(char *from, int size, char *buffer, unsigned int *pos)
{
	char str[10];
	memset(str, 0, 10);
	DataToHex(str, from, 4);

	//Reverse string
	char str2[10];
	memset(str2, 0, 10);
	int j = 9;
	for(int i = 0; i < 10; i += 2)
	{
		str2[i] = str[j - 1];
		str2[i + 1] = str[j];
		j -= 2;
	}

	//When copying back, start copying at the first non-'0'
	memset(str, 0, 10);
	bool copying = 0;
	j = 0;
	for(int i = 0; i < 10; i++)
	{
		if(str2[i] != '0' && str2[i] != 0)
			copying = 1;

		if(copying)
		{
			str[j] = str2[i];
			j++;
		}
	}
	if(!copying)
		str[0] = '0';

	//Add string to buffer
	j = 0;
	while(str[j] != 0)
	{
		buffer[*pos] = str[j];
		*pos += 1;
		j++;
	}
	buffer[*pos] = ':';
	*pos += 1;
}

unsigned int ReadStringValue(char *str, int *strPos)
{
	//Read string
	char str2[100];
	int pos = 0;
	while(str[*strPos] != ':')
	{
		str2[pos] = str[*strPos];
		pos++;
		*strPos += 1;
	}
	str2[pos] = 0;
	*strPos += 1; //Skip ':'

	//Convert string to value
	unsigned int value = 0;
	CharByteToData_Beta(str2, (unsigned char *) &value, 4);

	return value;
}

bool GenerateEntryList()
{
	//Load in fileList file
	unsigned char *buffer = new unsigned char[fileListSize];
	fread(buffer, fileListSize, 1, fileList);

	//Read header
	filelist_header_s *header = (filelist_header_s *) &buffer[0];
	fileNum = header->fileNum;

	//Error handling
	if(header->fileChunkEntryListOffset > header->fileChunksOffset ||
		header->fileChunkEntryListOffset > fileListSize ||
		header->fileChunksOffset > fileListSize
		)
	{
		//Check if this is a decrypted file list and skip the first 32 bytes
		header = (filelist_header_s *) &buffer[32];
		fileNum = header->fileNum;
		decrypted = 1;

		if(header->fileChunkEntryListOffset > header->fileChunksOffset ||
			header->fileChunkEntryListOffset > fileListSize ||
			header->fileChunksOffset > fileListSize
			)
		{
			Printf("Error: Header info incorrect. Not a valid filelist, or it's encrypted.\n");
			delete[]buffer;
			return 0;
		}
	}

	//Save encryption header in case we're rewriting filelist
	if(decrypted)
		memcpy(cryptHeader, buffer, 32);

	chunkNum = (header->fileChunksOffset - header->fileChunkEntryListOffset) / sizeof(filelist_fileChunkEntry_s);

	//Generate entry list
	entries = new entry_s[fileNum];
	memset(entries, 0, sizeof(entry_s) * fileNum);

	//Add basic entry list info
	unsigned int pos = sizeof(filelist_header_s);
	if(decrypted)
		pos += 32;

	short curChunk = -1;
	unsigned short maxOffset = 8243; //Highest size of filechunks I've seen in FF13-2. The actual value used by game might be different, but this seems to work.
	unsigned short curMinusOffset = 0;

	for(unsigned int i = 0; i < fileNum; i++)
	{
		if(game == GAME_FF13)
		{
			filelist_fileEntry_FF13_s *entry = (filelist_fileEntry_FF13_s *) &buffer[pos];

			entries[i].fileChunk = entry->fileChunk;
			entries[i].fileChunkOffset = entry->offsetInFileChunk;
			entries[i].unknown1 = entry->unknown1;
			entries[i].unknown2 = entry->unknown2;

			pos += sizeof(filelist_fileEntry_FF13_s);
		}
		else if(game == GAME_FF13_2)
		{
			filelist_fileEntry_FF13_2_s *entry = (filelist_fileEntry_FF13_2_s *) &buffer[pos];

			entries[i].unknown3 = entry->unknown3;
			entries[i].fileChunkOffset = entry->offsetInFileChunk;
			entries[i].unknown1 = entry->unknown1;
			entries[i].unknown2 = entry->unknown2;

			if(entries[i].fileChunkOffset == 0)
			{
				curChunk++;
				curMinusOffset = 0;
			}
			else if(entries[i].fileChunkOffset - curMinusOffset > maxOffset)
			{
				curChunk++;
				curMinusOffset += entries[i].fileChunkOffset;
			}
			entries[i].fileChunk = curChunk;
			entries[i].fileChunkOffset -= curMinusOffset;
			entries[i].offsetMod = curMinusOffset;

			pos += sizeof(filelist_fileEntry_FF13_2_s);
		}
	}

	//Generate fileChunk list
	fileChunks = new fileChunk_s[chunkNum];

	//Add info about fileChunks
	for(int i = 0; i < chunkNum; i++)
	{
		filelist_fileChunkEntry_s *entry = (filelist_fileChunkEntry_s *) &buffer[pos];

		fileChunks[i].compressedSize = entry->compressedSize;
		fileChunks[i].offset = entry->offset;
		fileChunks[i].realSize = entry->realSize;

		pos += sizeof(filelist_fileChunkEntry_s);
	}

	//Uncompress all fileChunks
	for(int i = 0; i < chunkNum; i++)
	{
		fileChunk_s *entry = &fileChunks[i];

		//Read compressed data
		char *data = new char[entry->compressedSize];
		fseek(fileList, header->fileChunksOffset + entry->offset, SEEK_SET);
		if(decrypted)
			fseek(fileList, 32, SEEK_CUR);
		fread(data, entry->compressedSize, 1, fileList);
		
		//Uncompress data
		entry->data = new char[entry->realSize];
		ezuncompress( (unsigned char *) entry->data, (long *) &entry->realSize, (unsigned char *) data, (long) entry->compressedSize);
		delete[]data;

		//Debugging, save file chunk data to individual file
		if(mode == MODE_RECREATEFILELIST)
		{
			char chunkname[100];
			sprintf_s(chunkname, 100, "read-chunk%i.bin", i);
			FILE *chunkFile;
			fopen_s(&chunkFile, chunkname, "wb");
			if(chunkFile)
			{
				fwrite(entry->data, entry->realSize, 1, chunkFile);
				fclose(chunkFile);
			}
		}
	}

	//Read in remaining file entry info from fileChunks
	for(unsigned int i = 0; i < fileNum; i++)
	{
		fileChunk_s *fileChunkEntry = &fileChunks[entries[i].fileChunk];
		char *str = fileChunkEntry->data;
		int strPos = entries[i].fileChunkOffset;

		//Read ints
		entries[i].offset = ReadStringValue(str, &strPos);
		entries[i].realSize = ReadStringValue(str, &strPos);
		entries[i].compressedSize = ReadStringValue(str, &strPos);

		//Read filepath
		int strPos2 = 0;
		while(str[strPos] != 0)
		{
			entries[i].fileName[strPos2] = str[strPos];
			strPos2++;
			strPos++;
		}
	}

	Printf("File list processed. %i files listed in %i file chunks.\n", fileNum, chunkNum);

	delete[]buffer;
	return 1;
}

bool ExtractAllFiles(char *archiveName)
{
	//Determine out directory
	char outDir[100];
	for(int i = 0; ; i++)
	{
		if(archiveName[i] == '.')
		{
			outDir[i] = 0;
			break;
		}
		outDir[i] = archiveName[i];
	}

	int compressedNum = 0;
	int unCompressedNum = 0;
	for(unsigned int i = 0; i < fileNum; i++)
	{
		entry_s *entry = &entries[i];

		//Determine full path
		char path[MAKEDIR_MAXSIZE];
		sprintf_s(path, MAKEDIR_MAXSIZE, "%s/%s", outDir, entry->fileName);

		//Make sure out directory exists
		MakeDirectory(path);

		//Read compressed data
		unsigned char *comp = new unsigned char[entry->compressedSize];
		fseek(fileArchive, entry->offset * 0x800, SEEK_SET);
		fread(comp, entry->compressedSize, 1, fileArchive);

		//Prepare file for writing
		FILE *file;
		fopen_s(&file, path, "wb");
		if(!file)
		{
			Printf("Error: Failed to open %s for writing\n", path);
			return 0;
		}
		
		bool compressed = 1;
		if(entry->compressedSize == entry->realSize) //This file is not compressed
		{
			//Copy data to new file
			fwrite(comp, entry->realSize, 1, file);
			unCompressedNum++;
			compressed = 0;
		}
		else
		{
			//Uncompress data
			unsigned char *unComp = new unsigned char[entry->realSize];
			ezuncompress(unComp, (long *) &entry->realSize, comp, (long) entry->compressedSize);

			//Copy data to new file
			fwrite(unComp, entry->realSize, 1, file);
			delete[]unComp;
			compressedNum++;
		}
		
		//Finish
		delete[]comp;
		fclose(file);
		if(compressed)
			Printf("Extracted %s (uncompressed)\n", path);
		else
			Printf("Extracted %s (copied)\n", path);
	}
	Printf("Extracted %i files in total.\n", compressedNum + unCompressedNum);
	Printf("%i files uncompressed. %i files copied.\n", compressedNum, unCompressedNum);
	return 1;
}

bool GenerateArchive(char *archiveDir)
{
	//Prepare archive file
	char archiveFilename[100];
	sprintf_s(archiveFilename, 100, "%s.win32.bin", archiveDir);
	fopen_s(&fileArchive, archiveFilename, "wb");
	if(!fileArchive)
	{
		Printf("Failed to open %s for writing\n", archiveFilename);
		return 0;
	}
	
	//Process file and compress them into archive
	unsigned int curOffset = 0;
	int compressedNum = 0;
	int copiedNum = 0;
	for(unsigned int i = 0; i < fileNum; i++)
	{
		entry_s *entry = &entries[i];

		//Determine full path
		char path[MAKEDIR_MAXSIZE];
		sprintf_s(path, MAKEDIR_MAXSIZE, "%s/%s", archiveDir, entry->fileName);

		//Open file
		FILE *file;
		fopen_s(&file, path, "rb");
		if(!file)
		{
			Printf("Error: Failed to open %s for reading\n", path);
			return 0;
		}

		//Read file
		fpos_t fpos;
		fseek(file, 0, SEEK_END);
		fgetpos(file, &fpos);
		fseek(file, 0, SEEK_SET);
		unsigned int fileSize = (unsigned int) fpos;
		unsigned char *unComp = new unsigned char[fileSize];
		fread(unComp, fileSize, 1, file);
		fclose(file);

		//Compress file data
		unsigned int compSize = EZ_COMPRESSMAXDESTLENGTH(fileSize);
		unsigned char *comp = new unsigned char[compSize];
		ezcompress(comp, (long *) &compSize, unComp, (long) fileSize);

		bool compressed = 0;
		if(	(strstr(entries[i].fileName, ".scd") || //SCD (sound data) should never be compressed
			strstr(entries[i].fileName, "filelist") ||  //Filelists should never be compressed either
			compSize >= fileSize) && fileSize != 0) //Compressed size is not smaller than original size, so directly copy file data instead
		{
			//Write uncompressed data into archive
			fwrite(unComp, fileSize, 1, fileArchive);
			compSize = fileSize;
			copiedNum++;
		}
		else
		{
			//Write compressed data into archive
			fwrite(comp, compSize, 1, fileArchive);
			compressedNum++;
			compressed = 1;
		}

		//Finish
		delete[]unComp;
		delete[]comp;
		entries[i].realSize = fileSize;
		entries[i].compressedSize = compSize;
		entries[i].offset = curOffset / 0x800;
		curOffset += compSize;
		if(compressed)
			Printf("Added %s (compressed)\n", path);
		else
			Printf("Added %s (copied)\n", path);

		//Check if we should add padding so we're still aligned
		if(curOffset % ALIGNMENT)
		{
			int size = ALIGNMENT - (curOffset % ALIGNMENT);
			unsigned char *padding = new unsigned char[size];
			memset(padding, 0, size);
			fwrite(padding, size, 1, fileArchive);
			curOffset += size;
		}
	}

	Printf("Added %i files in total.\n", compressedNum + copiedNum);
	Printf("%i files compressed. %i files copied.\n", compressedNum, copiedNum);
	Printf("Wrote %s\n", archiveFilename);
	fclose(fileArchive);
	return 1;
}

bool ImportFile(char *archiveFilename, char *filename)
{
	//Figure out size of archive
	fopen_s(&fileArchive, archiveFilename, "rb");
	if(!fileArchive)
	{
		Printf("Could not open %s for reading.\n", archiveFilename);
		return 0;
	}
	fpos_t fpos;
	fseek(fileArchive, 0, SEEK_END);
	fgetpos(fileArchive, &fpos);
	fseek(fileArchive, 0, SEEK_SET);
	unsigned int archiveSize = (unsigned int) fpos;
	fclose(fileArchive);

	//Open archive for writing (appending)
	fopen_s(&fileArchive, archiveFilename, "ab");
	if(!fileArchive)
	{
		Printf("Could not open %s for writing.\n", archiveFilename);
		return 0;
	}

	//Replace all \\ with / as that's what filenames in entry list expects
	unsigned int pos = 0;
	while(filename[pos] != 0)
	{
		if(filename[pos] == '\\')
			filename[pos] = '/';
		pos++;
	}

	//Create variant of filename which skips first dir
	char path[MAXPATH];
	bool copying = 0;
	pos = 0;
	unsigned int pos2 = 0;
	while(1)
	{
		if(copying)
		{
			path[pos2] = filename[pos];
			pos2++;
		}
		if(filename[pos] == '\\' || filename[pos] == '/')
			copying = 1;
		if(filename[pos] == 0)
			break;
		pos++;
	}

	bool foundEntry = 0;
	for(unsigned int i = 0; i < fileNum; i++)
	{
		if(_stricmp(entries[i].fileName, filename) == 0 || _stricmp(entries[i].fileName, path) == 0)
		{
			foundEntry = 1;

			//Check if we should add padding so we're still aligned
			if(archiveSize % ALIGNMENT)
			{
				int size = ALIGNMENT - (archiveSize % ALIGNMENT);
				unsigned char *padding = new unsigned char[size];
				memset(padding, 0, size);
				fwrite(padding, size, 1, fileArchive);
				archiveSize += size;
			}

			//Open file
			FILE *file;
			fopen_s(&file, filename, "rb");
			if(!file)
			{
				Printf("Error: Failed to open %s for reading\n", filename);
				return 0;
			}

			//Read file
			fseek(file, 0, SEEK_END);
			fgetpos(file, &fpos);
			fseek(file, 0, SEEK_SET);
			unsigned int fileSize = (unsigned int) fpos;
			unsigned char *unComp = new unsigned char[fileSize];
			fread(unComp, fileSize, 1, file);
			fclose(file);

			//Compress file data
			unsigned int compSize = EZ_COMPRESSMAXDESTLENGTH(fileSize);
			unsigned char *comp = new unsigned char[compSize];
			ezcompress(comp, (long *) &compSize, unComp, (long) fileSize);

			bool compressed = 0;
			if(	(strstr(entries[i].fileName, ".scd") || //SCD (sound data) should never be compressed
				strstr(entries[i].fileName, "filelist") ||  //Filelists should never be compressed either
				compSize >= fileSize) && fileSize != 0) //Compressed size is not smaller than original size, so directly copy file data instead
			{
				//Write uncompressed data into archive
				fwrite(unComp, fileSize, 1, fileArchive);
				compSize = fileSize;
			}
			else
			{
				//Write compressed data into archive
				fwrite(comp, compSize, 1, fileArchive);
				compressed = 1;
			}

			//Finish
			delete[]unComp;
			delete[]comp;
			entries[i].realSize = fileSize;
			entries[i].compressedSize = compSize;
			entries[i].offset = archiveSize / 0x800;
			if(compressed)
				Printf("Added %s (compressed)\n", path);
			else
				Printf("Added %s (copied)\n", path);
			break;
		}
	}

	fclose(fileArchive);
	if(!foundEntry)
	{
		Printf("Did not find file %s in archive.", filename);
		return 0;
	}
	Printf("Updated %s\n", archiveFilename);

	return 1;
}

bool GenerateFileList(char *fileListName)
{
	//Start creating the file chunks
	unsigned int sizeOfAllChunks = 0;
	for(int i = 0; i < chunkNum; i++)
	{
		//Calculate max possible size for this chunk
		unsigned int chunkSize = 0;
		for(unsigned int j = 0; j < fileNum; j++)
		{
			if(entries[j].fileChunk == i)
				chunkSize += strlen(entries[j].fileName) + 1; //Adding 1 as null terminator
		}
		chunkSize += fileNum * (9 * 3); //Add maximum space which ints will take

		//Create buffer for chunk data
		char *buffer = new char[chunkSize];
		unsigned int pos = 0;

		//Start processing entries for chunk
		for(unsigned int j = 0; j < fileNum; j++)
		{
			if(entries[j].fileChunk != i)
				continue;

			entries[j].fileChunkOffset = pos;

			//Convert ints into hexadecimal in text form
			ConvertIntToHexString( (char *) &entries[j].offset, 4, buffer, &pos);
			ConvertIntToHexString( (char *) &entries[j].realSize, 4, buffer, &pos);
			ConvertIntToHexString( (char *) &entries[j].compressedSize, 4, buffer, &pos);

			//Add filepath
			int k = 0;
			while(entries[j].fileName[k] != 0)
			{
				buffer[pos] = entries[j].fileName[k];
				pos++;
				k++;
			}
			buffer[pos] = 0;
			pos++;
		}

		//Debugging, save file chunk data to individual file
		if(mode == MODE_RECREATEFILELIST)
		{
			char chunkname[100];
			sprintf_s(chunkname, 100, "write-chunk%i.bin", i);
			FILE *chunkFile;
			fopen_s(&chunkFile, chunkname, "wb");
			if(chunkFile)
			{
				fwrite(buffer, pos, 1, chunkFile);
				fclose(chunkFile);
			}
		}

		//Compress chunk
		unsigned int compSize = EZ_COMPRESSMAXDESTLENGTH(pos);
		fileChunks[i].data = new char[compSize];
		ezcompress( (unsigned char *) fileChunks[i].data, (long *) &compSize, (unsigned char *) buffer, (long) pos);

		//Finish
		fileChunks[i].compressedSize = compSize;
		fileChunks[i].realSize = pos;
		fileChunks[i].offset = sizeOfAllChunks;
		sizeOfAllChunks += compSize;
	}

	//Determine full size of fileList
	unsigned int fileListSize = sizeof(filelist_header_s) + (sizeof(filelist_fileEntry_FF13_s) * fileNum) + (sizeof(filelist_fileChunkEntry_s) * chunkNum) + sizeOfAllChunks;

	//Create buffer for fileList
	unsigned char *buffer = new unsigned char[fileListSize];
	unsigned int pos = 0;

	//Header
	filelist_header_s *header = (filelist_header_s *) &buffer[pos];
	header->fileNum = fileNum;
	header->fileChunkEntryListOffset = sizeof(filelist_header_s) + (sizeof(filelist_fileEntry_FF13_s) * fileNum);
	header->fileChunksOffset = header->fileChunkEntryListOffset + (sizeof(filelist_fileChunkEntry_s) * chunkNum);
	pos += sizeof(filelist_header_s);

	//File entry list
	for(unsigned int i = 0; i < fileNum; i++)
	{
		if(game == GAME_FF13)
		{
			filelist_fileEntry_FF13_s *entry = (filelist_fileEntry_FF13_s *) &buffer[pos];

			entry->fileChunk = entries[i].fileChunk;
			entry->offsetInFileChunk = entries[i].fileChunkOffset;
			entry->unknown1 = entries[i].unknown1;
			entry->unknown2 = entries[i].unknown2;

			pos += sizeof(filelist_fileEntry_FF13_s);
		}
		else if(game == GAME_FF13_2)
		{
			filelist_fileEntry_FF13_2_s *entry = (filelist_fileEntry_FF13_2_s *) &buffer[pos];

			entry->unknown1 = entries[i].unknown1;
			entry->unknown2 = entries[i].unknown2;
			entry->offsetInFileChunk = entries[i].fileChunkOffset + entries[i].offsetMod;
			entry->unknown3 = entries[i].unknown3;

			pos += sizeof(filelist_fileEntry_FF13_2_s);
		}
	}

	//Chunk entry list
	for(int i = 0; i < chunkNum; i++)
	{
		filelist_fileChunkEntry_s *entry = (filelist_fileChunkEntry_s *) &buffer[pos];

		entry->compressedSize = fileChunks[i].compressedSize;
		entry->realSize = fileChunks[i].realSize;
		entry->offset = fileChunks[i].offset;

		pos += sizeof(filelist_fileChunkEntry_s);
	}

	//Copy chunks
	for(int i = 0; i < chunkNum; i++)
	{
		for(unsigned j = 0; j < fileChunks[i].compressedSize; j++)
		{
			buffer[pos] = fileChunks[i].data[j];
			pos++;
		}
		delete[]fileChunks[i].data;
	}

	if(pos != fileListSize)
		Printf("Error: Miscalculation!");

	//Create file
	fopen_s(&fileList, fileListName, "wb");
	if(!fileList)
	{
		Printf("Failed to open %s for writing\n", fileListName);
		return 0;
	}
	if(decrypted)
		fwrite(cryptHeader, 1, 32, fileList);
	fwrite(buffer, fileListSize, 1, fileList);
	fclose(fileList);
	delete[]buffer;

	Printf("Wrote %s\n", fileListName);
	return 1;
}

int _tmain(int argc, _TCHAR* argv[])
{
	CreateLog();

	game = -1;
	mode = MODE_NOTHING;
	int i = 1;
	while(argc>i && argv[i][0] == '-')
	{
		if(_stricmp(argv[i], "-x") == 0)
			mode = MODE_READ;
		else if(_stricmp(argv[i], "-c") == 0)
			mode = MODE_WRITE;
		else if(_stricmp(argv[i], "-i") == 0)
			mode = MODE_IMPORT;
		else if(_stricmp(argv[i], "-recreatefilelist") == 0)
			mode = MODE_RECREATEFILELIST;
		else if(_stricmp(argv[i], "-l") == 0)
			mode = MODE_LIST;
		else if(_stricmp(argv[i], "-h") == 0 || strcmp(argv[i], "-?") == 0)
			mode = MODE_HELP;
		else if(_stricmp(argv[i], "-ff13") == 0)
			game = GAME_FF13;
		else if(_stricmp(argv[i], "-ff132") == 0)
			game = GAME_FF13_2;
		i++;
	}

	if(game == -1)
	{
		Printf("Game automatically set to FF13.\n");
		game = GAME_FF13;
	}
	else if(game == GAME_FF13)
		Printf("Game set to FF13.\n");
	else if(game == GAME_FF13_2)
		Printf("Game set to FF13-2.\n");

	if(mode == MODE_READ && argc >= i + 2)
	{
		//Prepare files
		fopen_s(&fileList, argv[i], "rb");
		if(!fileList)
		{
			Printf("Failed to open %s for reading\n", argv[i]);
			goto finish;
		}
		fpos_t fileSize;
		fseek(fileList, 0, SEEK_END);
		fgetpos(fileList, &fileSize);
		fseek(fileList, 0, SEEK_SET);
		fileListSize = (unsigned int) fileSize;

		fopen_s(&fileArchive, argv[i + 1], "rb");
		if(!fileArchive)
		{
			Printf("Failed to open %s for reading\n", argv[i]);
			goto finish;
		}

		//Process files
		if(GenerateEntryList())
			ExtractAllFiles(argv[i + 1]);

		//Finish
		fclose(fileList);
		fclose(fileArchive);
		delete[]entries;
		for(int i = 0; i < chunkNum; i++)
			delete[]fileChunks[i].data;
		delete[]fileChunks;
	}
	else if(mode == MODE_WRITE && argc >= i + 2)
	{
		//Prepare file list
		fopen_s(&fileList, argv[i], "rb");
		if(!fileList)
		{
			Printf("Failed to open %s for reading\n", argv[i]);
			goto finish;
		}
		fpos_t fileSize;
		fseek(fileList, 0, SEEK_END);
		fgetpos(fileList, &fileSize);
		fseek(fileList, 0, SEEK_SET);
		fileListSize = (unsigned int) fileSize;

		//Read in file entry list
		if(!GenerateEntryList())
			goto finish;
		fclose(fileList);
		for(int j = 0; j < chunkNum; j++)
			delete[]fileChunks[j].data;

		//Create new archive container file
		if(!GenerateArchive(argv[i + 1]))
			goto finish;

		//Create new fileList file
		GenerateFileList(argv[i]);

		//Finish
		delete[]entries;
		delete[]fileChunks;
	}
	else if(mode == MODE_IMPORT && argc >= i + 3)
	{
		//Prepare file list
		fopen_s(&fileList, argv[i], "rb");
		if(!fileList)
		{
			Printf("Failed to open %s for reading\n", argv[i]);
			goto finish;
		}
		fpos_t fileSize;
		fseek(fileList, 0, SEEK_END);
		fgetpos(fileList, &fileSize);
		fseek(fileList, 0, SEEK_SET);
		fileListSize = (unsigned int) fileSize;

		//Read in file entry list
		if(!GenerateEntryList())
			goto finish;
		fclose(fileList);
		for(int j = 0; j < chunkNum; j++)
			delete[]fileChunks[j].data;

		//Import file
		if(!ImportFile(argv[i + 1], argv[i + 2]))
			goto finish;

		//Create new fileList file
		GenerateFileList(argv[i]);

		//Finish
		delete[]entries;
		delete[]fileChunks;
	}
	else if(mode == MODE_RECREATEFILELIST && argc >= i + 1)
	{
		//Prepare file list
		fopen_s(&fileList, argv[i], "rb");
		if(!fileList)
		{
			Printf("Failed to open %s for reading\n", argv[i]);
			goto finish;
		}
		fpos_t fileSize;
		fseek(fileList, 0, SEEK_END);
		fgetpos(fileList, &fileSize);
		fseek(fileList, 0, SEEK_SET);
		fileListSize = (unsigned int) fileSize;

		//Read in file entry list
		if(!GenerateEntryList())
			goto finish;
		fclose(fileList);
		for(int j = 0; j < chunkNum; j++)
			delete[]fileChunks[j].data;

		//Create new fileList file
		GenerateFileList(argv[i]);

		//Finish
		delete[]entries;
		delete[]fileChunks;
	}
	else if(mode == MODE_LIST && argc >= i + 1)
	{
		//sprintf(filenameKOF,"%s",argv[i]);
		//SearchArcFile();
	}
	else if(mode == MODE_HELP)
	{
		ExtendedHelpText();
	}
	else
		mode = MODE_NOTHING;

	if(mode == MODE_NOTHING)
	{
		HelpText();
	}
finish:
	CloseLog();
	return 1;
}