import random
import struct
import binascii
from abc import abstractmethod
from . import algo
from . import logger

# 默认的 ID 取值范围
RM_SDK_FIRST_SEQ_ID = 10000
RM_SDK_LAST_SEQ_ID = 20000

# 协议 ACK 类型
DUSS_MB_ACK_NO = 0
DUSS_MB_ACK_NOW = 1
DUSS_MB_ACK_FINISH = 2

# 协议加密类型
DUSS_MB_ENC_NO = 0
DUSS_MB_ENC_AES128 = 1
DUSS_MB_ENC_CUSTOM = 2

# 协议类型
DUSS_MB_TYPE_REQ = 0
DUSS_MB_TYPE_PUSH = 1

def host2byte(host, index):
    return index * 32 + host


def byte2host(b):
    return (b & 0x1f), (b >> 5)


def make_proto_cls_key(cmdset, cmdid):
    return cmdset * 256 + cmdid

# registered protocol dict.
registered_protos = {}

def make_proto_cls_key(cmdset, cmdid):
    return cmdset * 256 + cmdid

class _AutoRegisterProto(type):
    """ help to automatically register Proto Class where ever they're defined """

    def __new__(mcs, name, bases, attrs, **kw):
        return super().__new__(mcs, name, bases, attrs, **kw)

    def __init__(cls, name, bases, attrs, **kw):
        super().__init__(name, bases, attrs, **kw)
        if name == 'ProtoData':
            return
        key = make_proto_cls_key(attrs['_cmdset'], attrs['_cmdid'])
        if key in registered_protos.keys():
            raise ValueError("Duplicate proto class %s" % (name))
        registered_protos[key] = cls

class ProtoData(metaclass=_AutoRegisterProto):
    _cmdset = None
    _cmdid = None
    _cmdtype = DUSS_MB_TYPE_REQ
    _req_size = 0
    _resp_size = 0

    def __init__(self, **kwargs):
        self._buf = None
        self._len = None

    def __repr__(self):
        return "<{0} cmset:0x{1:2x}, cmdid:0x{2:02x}>".format(self.__class__.__name__, self._cmdset, self._cmdid)

    @property
    def cmdset(self):
        return self._cmdset

    @cmdset.setter
    def cmset(self, value):
        self._cmdset = value

    @property
    def cmdid(self):
        return self._cmdid

    @cmdid.setter
    def cmdid(self, value):
        self._cmdid = value

    @property
    def cmdkey(self):
        if self._cmdset is not None and self._cmdid is not None:
            return self._cmdset * 256 + self._cmdid
        else:
            return None

    @abstractmethod
    def pack_req(self):
        """ 协议对象打包发送数据为字节流

        :return: 字节流数据
        """
        return b''

    # @abstractmethod
    def unpack_req(self, buf, offset=0):
        """ 从字节流解包

        :param buf：字节流数据
        :param offset：字节流数据偏移量
        :return：True 解包成功；False 解包失败
        """
        return True

    # @abstractmethod
    def pack_resp(self):
        """ 协议对象打包

        :return：字节流数据
        """
        pass

    # return True when retcode == zero
    # return False when restcode is not zero
    # raise exceptions when internal error occur.
    def unpack_resp(self, buf, offset=0):
        """ 从字节流解包为返回值和相关属性

        :param buf：字节流数据
        :param offset：字节流数据偏移量
        :return: bool: 调用结果
        """
        self._retcode = buf[offset]
        if self._retcode == 0:
            return True
        else:
            return False
