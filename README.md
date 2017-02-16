- - - 
## Personal snipping tool - OCR newspaper reader
- - -

_**Faculty of Tehnical Sciences**, Applied computer sciences Novi Sad_

_**Course**: Soft computing 2016/17_

_**Authors**: Zlatan Precanica, Jasmina Eminovski_
___________________________________________________________________________________________________________

# Idea 

**OCR** (Optical Character Recognition) is process of converting different type of documents, scanned paper documents, PDFs, images into editable and searchable data.
This technology is very popular and in widespread usage. It can recognize characters, words and sentences without making many mistakes. Although, documents containing images besides text may be not converted completley. 

# Motivation

In the running world there is a growing demand for software systems to recognize characters in computer system when information is scanned through paper document. The question is how to find valuable data from all those information and store it. The concept of storing the contents of paper documents in computer storage place and then reading and searching content is called _document processing_.
Motivation for this project is to make personal snipping tool in the role of _online newspaper reader_. Processing of online document content has challanges. Detect language, separate text from other information (images) and read it.
This project is mainly a research of [Tesseract](https://github.com/tesseract-ocr) engine and its 
[wrapper](https://github.com/charlesw/tesseract) for .NET  framework.

# Realization

The process of developing started with making small and useful tool with a purpose of snipping images. With this tool user will be able to separate interesting digital content. Next step was integration of Tesseract engine. Firstly was used [tessnet2](http://www.pixel-technology.com/freeware/tessnet2/) library which didn't have support for many languages. Therefore, itegration moved to Tesseract 3.0.2 version. This engine can be trained to recognize specific character languages and writing directions, but it also has a large amount of language data.

Steps in OCR:

1. Input image

2. Adaptive thresholding (making binary image)

3. Connect component analysis

4. Find lines and words (character outlines)

5. Recognize words (pass I)

6. Recognize words (pass II)

7. Extract text from image

Two passes for recognizing words are neccessery because in the first one words and recognized characters are passed to an adaptive classifier, which uses data as training data. The second pass is for the text to be recognized but now  using adaptive classifier, previously trained.

Some images based on text font and size give better results. For every passed image there is an average accuracy in recognized text. In addition of validation it is used Levenstain distance algorithm for comparing words.

_Example_:

- Words: ant(recognized), aunt(passed);
- Levenstein distance: 1;
- Comment: Only 1 edit is needed. The 'u' must be added at index 2.


# Requirements
- System requirements: OS Win7 or Win10

- Techinal requirements: 
	
	1.Possible Internet connection
	
	2.Installed Speech platform V11
	
	3.Installed support for other languages(English, German, Italian)


#Posible improvements

The field of OCR and Tesseract engine is constantly growing, so this could be starter point for some bigger projects. The question is, are there any limits? Can Tesseract engine be trained so well to recognize any font or size of the letters and other signs. Combining neural networks and this engine will be possible next step in this research.

![IMG](http://i67.tinypic.com/2nir2bs.png[/IMG])
![IMG](http://i67.tinypic.com/2ij3znq.png[/IMG])
