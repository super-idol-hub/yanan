"""Render the original YA line-mark as a Windows multi-size application icon."""
from pathlib import Path
from PIL import Image, ImageDraw

canvas = Image.new('RGBA', (256, 256))
draw = ImageDraw.Draw(canvas)
draw.rounded_rectangle((4, 4, 252, 252), radius=56, fill='#171a20')
draw.line([(42, 72), (78, 124), (114, 72)], fill='white', width=14, joint='curve')
draw.line([(78, 124), (78, 182)], fill='white', width=14)
draw.line([(130, 182), (173, 72), (216, 182)], fill='white', width=14, joint='curve')
draw.line([(145, 145), (202, 145)], fill='white', width=12)
canvas.save(Path(__file__).with_name('yanan.ico'), sizes=[(16,16),(24,24),(32,32),(48,48),(64,64),(128,128),(256,256)])
