﻿namespace MexTK.FighterFunction
{
    public enum RelocType
    {
        R_PPC_NONE,
        R_PPC_ADDR32,
        R_PPC_ADDR24,
        R_PPC_ADDR16,
        R_PPC_ADDR16_LO,
        R_PPC_ADDR16_HI,
        R_PPC_ADDR16_HA,
        R_PPC_ADDR14,
        R_PPC_ADDR14_BRTAKEN,
        R_PPC_ADDR14_BRNTAKEN,
        R_PPC_REL24,
        R_PPC_REL14,
        R_PPC_REL14_BRTAKEN,
        R_PPC_REL14_BRNTAKEN,
        R_PPC_GOT16,
        R_PPC_GOT16_LO,
        R_PPC_GOT16_HI,
        R_PPC_GOT16_HA,
        R_PPC_PLTREL24,
        R_PPC_COPY,
        R_PPC_GLOB_DAT,
        R_PPC_JMP_SLOT,
        R_PPC_RELATIVE,
        R_PPC_LOCAL24PC,
        R_PPC_UADDR32,
        R_PPC_UADDR16,
        R_PPC_REL32,
        R_PPC_PLT32,
        R_PPC_PLTREL32,
        R_PPC_PLT16_LO,
        R_PPC_PLT16_HI,
        R_PPC_PLT16_HA,
        R_PPC_SDAREL16,
        R_PPC_SECTOFF,
        R_PPC_SECTOFF_LO,
        R_PPC_SECTOFF_HI,
        R_PPC_SECTOFF_HA,
        R_PPC_COUNT
    }

    public enum SectionType
    {
        SHT_NULL = 0x00,
        SHT_PROGBITS = 0x01,
        SHT_SYMTAB = 0x02,
        SHT_STRTAB = 0x03,
        SHT_RELA = 0x04,
        SHT_HASH = 0x05,
        SHT_DYNAMIC = 0x06,
        SHT_NOTE = 0x07,
        SHT_NOBITS = 0x08,
        SHT_REL = 0x09,
        SHT_SHLIB = 0x0A,
        SHT_DYNSYM = 0x0b,
        SHT_INIT_ARRAY = 0x0E,
        SHT_FINI_ARRAY = 0x0F,
        SHT_PREINIT_ARRAY = 0x10,
        SHT_GROUP = 0x11,
        SHT_SYMTAB_SHNDX = 0x12,
        SHT_NUM = 0x13,
        SHT_LOOS = 0x60000000
    }

    public enum SymbolBinding
    {
        STB_LOCAL = 0,
        STB_GLOBAL = 1,
        STB_WEAK = 2,
        STB_LOOS = 10,
        STB_HIOS = 12,
        STB_LOPROC = 13,
        STB_HIPROC = 15
    }

    public enum SymbolType
    {
        STT_NOTYPE = 0,
        STT_OBJECT = 1,
        STT_FUNC = 2,
        STT_SECTION = 3,
        STT_FILE = 4,
        STT_COMMON = 5,
        STT_LOOS = 10,
        STT_HIOS = 12,
        STT_LOPROC = 13,
        STT_SPARC_REGISTER = 13,
        STT_HIPROC = 15
    }
}
