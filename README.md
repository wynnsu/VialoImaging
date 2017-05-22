# VialoImaging
CNTK ConvNet Example and C# Eval Example put into practice

#### **Requirements:**
* [Visual Studio 2017 Community](https://www.visualstudio.com/downloads/)
	* P.S. Make sure to check **.NET Desktop Development** when installing
* [Anaconda Python 3.5 Environment](https://www.continuum.io/downloads)
* [CNTK Nuget Package](https://www.nuget.org/packages/CNTK.CPUOnly/2.0.0-rc2)

## Prepare dataset
To make use of CNTK ConvNet example which targets on [CIFAR-10 Dataset](https://www.cs.toronto.edu/~kriz/cifar.html), we need to convert our image to matching format: 32x32 colour.
1. Resize the image to 32x32
2. Label images.Save images in different folders based on labels (Vialo.DataGenerator).

<img src="/labeling.jpg" width="200">

`string noisePath = System.IO.Path.GetDirectoryName(path) + @"/0/";`

`string backgroundPath = System.IO.Path.GetDirectoryName(path) + @"/1/";`

3. Generate mapfile. Use 75% data for traning and 25% for testing. Format: [filepath label]

e.g.: `c:\...\VialoImaging\Datasets\1\7280.bmp	1`

[Sample mapfile]("/Datasets/CIFAR-10/test_map.txt")

## Train your model
Simply run Datasets/ConvNet.py which provided by [CNTK example](https://github.com/Microsoft/CNTK/tree/master/Examples/Image/Classification/ConvNet/Python) with mapfile path and model output path

## Evaluate images
Refer to Vialo.TestEval project
1. Provide pre-trained model (*.dnn) path and target image path.
1. Call `var result = new VialoImage().Init(imagePath).Evaluate(modelPath);`
<img src="/targets.png" height="200">
