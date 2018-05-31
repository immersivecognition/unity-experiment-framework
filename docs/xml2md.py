"""
Python3 script used to generate UXF .md documentation from a .xml documentation tree 
"""

import markdown_generator as mg
import xml.etree.ElementTree as ET
import sys


# def to_md(xml_member):

#     member_name = xml_member.attrib['name']



#     with open()
#     writer = mg.Writer(f)

#     print(xml_member.attrib)
#     for param in xml_member:
#         print(param)
#     print("--")
        

class DocParser(object):

    def __init__(self, type_member):
        self.type_member = type_member

    def add_child(self, member_name, symbol_type, symbol_path, child_member):
        if symbol_type == "M":
            pass
        elif symbol_type == "F":
            pass
        elif symbol_type == "P":
            pass



if __name__ == "__main__":
    filepath = sys.argv[1]
    tree = ET.parse(filepath)
    root = tree.getroot()
    
    types_dict = {}


    # find all Types
    for child in root[1]:
        if child.tag != "member":
            continue
        member_name = child.attrib['name']
        symbol_type, symbol = member_name.split(":")
        symbol_path = symbol.split(".")
        if symbol_type != "T" or len(symbol_path) == 1 or symbol_path[0] != "UXF":
            continue
        types_dict[symbol_path[1]] = DocParser(child);
    
    # add Methods, Fields and Properties to Types
    for child in root[1]:
        if child.tag != "member":
            continue
        member_name = child.attrib['name']
        symbol_type, symbol = member_name.split(":")
        symbol_path = symbol.split(".")
        if symbol_type == "T" or len(symbol_path) == 1 or symbol_path[0] != "UXF":
            associated_class = symbol_path[1]
            types_dict[associated_class].add_child(
                member_name, symbol_type, symbol_path, child)

            
        
        
        

        


