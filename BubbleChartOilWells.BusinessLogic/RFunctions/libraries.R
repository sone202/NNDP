# установка и загрузка библиотек

#if (!require("installr")) install.packages("installr", dependencies = TRUE);
library(installr)
#install.Rtools(check = TRUE, check_r_update = TRUE, GUI = TRUE);

#if (!require("devtools")) install.packages("devtools", dependencies = TRUE);
library(devtools)
#if (!require("neuralnet")) install_github("bips-hb/neuralnet");
library(neuralnet)

if (!require("stats")) install.packages("stats", dependencies = TRUE);
library(stats)
if (!require("tidyverse")) install.packages("tidyverse", dependencies = TRUE);
library(tidyverse)
if (!require("GGally")) install.packages("GGally", dependencies = TRUE);
library(GGally)
if (!require("Rsagacmd")) install.packages("Rsagacmd", dependencies = TRUE);
library(Rsagacmd)

